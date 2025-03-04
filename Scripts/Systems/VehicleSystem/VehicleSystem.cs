﻿using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
using AtomicTorch.CBND.CoreMod.Helpers.Client;
using AtomicTorch.CBND.CoreMod.StaticObjects;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
using AtomicTorch.CBND.CoreMod.Systems.Faction;
using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
using AtomicTorch.CBND.CoreMod.Triggers;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using AtomicTorch.GameEngine.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
  public partial class VehicleSystem : ProtoSystem<VehicleSystem>
  {
    public const string Notification_CannotUseVehicle_TitleFormat = "Cannot use {0}";

    public static readonly SoundResource SoundResourceVehicleBuilt
        = new("Objects/Structures/ObjectVehicleAssemblyBay/VehicleBuilt");

    public static readonly SoundResource SoundResourceVehicleRepair
        = new("Objects/Structures/ObjectVehicleAssemblyBay/VehicleRepair");

    private static readonly Dictionary<ICharacter, IDynamicWorldObject> ServerVehicleQuitRequests
        = IsServer ? new Dictionary<ICharacter, IDynamicWorldObject>() : null;

    public static void ClientOnVehicleEnterOrExitRequest()
    {
      Instance.ClientOnVehicleEnterExitButtonPress();
    }

    public static void ServerCharacterExitCurrentVehicle(ICharacter character, bool force)
    {
      if (character.IsNpc)
      {
        return;
      }

      var vehicle = PlayerCharacter.GetPublicState(character).CurrentVehicle;
      if (vehicle is null)
      {
        return;
      }

      if (force)
      {
        ServerCharacterExitVehicleNow(character, vehicle);
        return;
      }

      ServerCharacterExitVehicle(character, vehicle);
    }

    public static void ServerOnVehicleDestroyed(IDynamicWorldObject vehicle)
    {
      var publicState = vehicle.GetPublicState<VehiclePublicState>();
      if (publicState.PilotCharacter is not null)
      {
        ServerCharacterExitVehicle(publicState.PilotCharacter, vehicle);
      }

      ServerResetLastVehicleMapMark(
          vehicle.GetPrivateState<VehiclePrivateState>());
    }

    public static void ServerResetLastVehicleMapMark(VehiclePrivateState vehiclePrivateState)
    {
      if (vehiclePrivateState.ServerLastPilotCharacter is null)
      {
        return;
      }

      var characterPrivateState = PlayerCharacter.GetPrivateState(vehiclePrivateState.ServerLastPilotCharacter);
      characterPrivateState.LastDismountedVehicleMapMark = default;
      vehiclePrivateState.ServerLastPilotCharacter = null;
    }

    public static IStaticWorldObject SharedFindVehicleAssemblyBayNearby(ICharacter character)
    {
      using var tempStaticObjects = Api.Shared.GetTempList<IStaticWorldObject>();
      if (IsServer)
      {
        Server.World.GetStaticWorldObjectsInView(character,
                                                 tempStaticObjects.AsList(),
                                                 sortByDistance: false);
      }
      else
      {
        Client.World.GetStaticWorldObjects(tempStaticObjects.AsList());
      }

      foreach (var staticWorldObject in tempStaticObjects.AsList())
      {
        if (staticWorldObject.ProtoStaticWorldObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay
            && protoVehicleAssemblyBay.SharedCanInteract(character, staticWorldObject, writeToLog: false))
        {
          return staticWorldObject;
        }
      }

      return null;
    }

    protected override void PrepareSystem()
    {
      if (IsClient)
      {
        ClientInputContext.Start("Enter/Exit vehicle")
                          .HandleButtonDown(GameButton.VehicleEnterExit,
                                            this.ClientOnVehicleEnterExitButtonPress);
      }
      else
      {
        TriggerEveryFrame.ServerRegister(ServerProcessVehicleQuitRequests,
                                         nameof(VehicleSystem));

        FactionSystem.ServerFactionDissolved += ServerFactionDissolvedHandler;
      }
    }

    private static void ServerCharacterExitVehicle(ICharacter character, IDynamicWorldObject vehicle)
    {
      var characterPublicState = PlayerCharacter.GetPublicState(character);
      if (characterPublicState.CurrentVehicle != vehicle)
      {
        return;
      }

      var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
      if (vehiclePublicState.IsDismountRequested)
      {
        // quit is already requested
        return;
      }

      if (vehicle.PhysicsBody.Velocity.LengthSquared > 0)
      {
        // cannot quit now, check later
        ServerVehicleQuitRequests[character] = vehicle;
        vehiclePublicState.IsDismountRequested = true;
        return;
      }

      ServerCharacterExitVehicleNow(character, vehicle);
    }

    private static void ServerCharacterExitVehicleNow(ICharacter character, IDynamicWorldObject vehicle)
    {
      if (ServerVehicleQuitRequests.TryGetValue(character, out var requestedVehicleToQuit))
      {
        ServerVehicleQuitRequests.Remove(character);
        requestedVehicleToQuit.GetPublicState<VehiclePublicState>()
                              .IsDismountRequested = false;
      }

      var characterPublicState = PlayerCharacter.GetPublicState(character);
      if (characterPublicState.CurrentVehicle != vehicle)
      {
        return;
      }

      InteractableWorldObjectHelper.ServerTryAbortInteraction(character, vehicle);

      vehicle.GetPublicState<VehiclePublicState>()
             .IsDismountRequested = false;

      characterPublicState.ServerSetCurrentVehicle(null);

      var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
      vehiclePublicState.PilotCharacter = null;
      Logger.Important("Player exit vehicle: " + vehicle, character);

      if (!vehicle.IsDestroyed)
      {
        var characterPrivateState = PlayerCharacter.GetPrivateState(character);
        characterPrivateState.LastDismountedVehicleMapMark = new LastDismountedVehicleMapMark(vehicle);
      }

      character.ProtoWorldObject.SharedCreatePhysics(character);

      var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
      if (!vehicle.IsDestroyed)
      {
        vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
        // ensure vehicle stopped
        Server.World.SetDynamicObjectPhysicsMovement(vehicle, Vector2D.Zero, targetVelocity: 0);
        Server.World.SetDynamicObjectMoveSpeed(vehicle, 0);
        Server.World.StopPhysicsBody(vehicle.PhysicsBody);
        // no need to do this here, as it's done automatically by vehicle's ServerUpdate method
        //protoVehicle.ServerSetUpdateRate(vehicle, isRare: true);
        protoVehicle.ServerOnCharacterExitVehicle(vehicle, character);
      }

      PlayerCharacter.SharedForceRefreshCurrentItem(character);

      if (vehicle.IsDestroyed)
      {
        return;
      }

      // notify player and other players in scope
      using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(vehicle, tempPlayers);
      tempPlayers.Remove(character);

      Instance.CallClient(character,
                          _ => _.ClientRemote_OnVehicleExitByCurrentPlayer(vehicle, protoVehicle));
      Instance.CallClient(tempPlayers.AsList(),
                          _ => _.ClientRemote_OnVehicleExitByOtherPlayer(vehicle,
                                                                         vehicle.Position,
                                                                         protoVehicle));
    }

    private static void ServerProcessVehicleQuitRequests()
    {
      ServerVehicleQuitRequests.ProcessAndRemoveByValue(
          removeCondition: v => v.IsDestroyed
                                || v.PhysicsBody is null
                                || v.PhysicsBody.Velocity.LengthSquared == 0,
          removeCallback: p => ServerCharacterExitVehicleNow(character: p.Key,
                                                             vehicle: p.Value));
    }

    private void ClientOnVehicleEnterExitButtonPress()
    {
      var character = ClientCurrentCharacterHelper.Character;
      if (character is null)
      {
        return;
      }

      var vehicle = ClientCurrentCharacterHelper.PublicState.CurrentVehicle;
      if (vehicle is not null)
      {
        // already inside a vehicle
        var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
        vehiclePublicState.IsDismountRequested = true;
        this.CallServer(_ => _.ServerRemote_ExitVehicle(vehicle));
        return;
      }

      if (InteractionCheckerSystem.SharedGetCurrentInteraction(character) is IDynamicWorldObject
              currentlyInteractingWithDynamicWorldObject
          && currentlyInteractingWithDynamicWorldObject.ProtoGameObject is IProtoVehicle)
      {
        vehicle = currentlyInteractingWithDynamicWorldObject;
      }
      else if (ClientComponentObjectInteractionHelper.MouseOverObject
                   is IDynamicWorldObject mouseOverWorldObject
               && mouseOverWorldObject.ProtoGameObject is IProtoVehicle)
      {
        vehicle = mouseOverWorldObject;
      }

      if (vehicle is null)
      {
        // no vehicle in interaction or under mouse cursor - try find a vehicle nearby
        var closestVehicle = Client.World.GetGameObjectsOfProto<IDynamicWorldObject, IProtoVehicle>()
                                   .OrderBy(v => v.Position.DistanceSquaredTo(character.Position))
                                   .FirstOrDefault();
        if (closestVehicle is not null
            && closestVehicle.Position.DistanceTo(character.Position) < 3)
        {
          vehicle = closestVehicle;
        }
      }

      if (vehicle is null)
      {
        // no vehicle to enter
        return;
      }

      if (!vehicle.ProtoWorldObject.SharedCanInteract(character, vehicle, writeToLog: true))
      {
        // cannot interact with this vehicle
        return;
      }

      WindowObjectVehicle.CloseActiveMenu();
      vehicle.GetPublicState<VehiclePublicState>()
             .IsDismountRequested = false;
      this.CallServer(_ => _.ServerRemote_EnterVehicle(vehicle));
    }

    [RemoteCallSettings(DeliveryMode.ReliableSequenced, groupName: "CurrentPlayerEnterExitVehicle")]
    private void ClientRemote_OnVehicleEnterByCurrentPlayer(IProtoVehicle protoVehicle)
    {
      Api.Client.Audio.PlayOneShot(
          protoVehicle.SoundResourceVehicleMount);
    }

    [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
    private void ClientRemote_OnVehicleEnterByOtherPlayer(Vector2D position, IProtoVehicle protoVehicle)
    {
      Api.Client.Audio.PlayOneShot(
          protoVehicle.SoundResourceVehicleMount,
          position + protoVehicle.SharedGetObjectCenterWorldOffset(null));
    }

    [RemoteCallSettings(DeliveryMode.ReliableSequenced, groupName: "CurrentPlayerEnterExitVehicle")]
    private void ClientRemote_OnVehicleExitByCurrentPlayer(IDynamicWorldObject vehicle, IProtoVehicle protoVehicle)
    {
      Api.Client.Audio.PlayOneShot(
          protoVehicle.SoundResourceVehicleDismount);

      ClientTimersSystem.AddAction(
          delaySeconds: 0,
          () =>
          {
            if (vehicle?.IsInitialized ?? false)
            {
              protoVehicle.ClientOnVehicleDismounted(vehicle);
            }
          });
    }

    [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
    private void ClientRemote_OnVehicleExitByOtherPlayer(
        IDynamicWorldObject vehicle,
        Vector2D position,
        IProtoVehicle protoVehicle)
    {
      Api.Client.Audio.PlayOneShot(
          protoVehicle.SoundResourceVehicleDismount,
          position + protoVehicle.SharedGetObjectCenterWorldOffset(null));

      ClientTimersSystem.AddAction(
          delaySeconds: 0,
          () =>
          {
            if (vehicle?.IsInitialized ?? false)
            {
              protoVehicle.ClientOnVehicleDismounted(vehicle);
            }
          });
    }

    private void ServerRemote_EnterVehicle(IDynamicWorldObject vehicle)
    {
      var character = ServerRemoteContext.Character;
      if (vehicle.ProtoGameObject is not IProtoVehicle protoVehicle)
      {
        throw new Exception("Not a vehicle");
      }

      var characterPublicState = PlayerCharacter.GetPublicState(character);
      if (characterPublicState.CurrentPublicActionState is CharacterTeleportAction.PublicState)
      {
        throw new Exception("Cannot enter a vehicle while teleporting");
      }

      if (!vehicle.ProtoWorldObject.SharedCanInteract(character,
                                                      vehicle,
                                                      writeToLog: true))
      {
        return;
      }

      if (characterPublicState.CurrentVehicle is not null)
      {
        Logger.Warning($"Player cannot enter vehicle: {vehicle} (already in a vehicle)", character);
        return;
      }

      var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
      var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
      if (vehiclePublicState.PilotCharacter is not null)
      {
        Logger.Warning($"Player cannot enter vehicle: {vehicle} (already has a pilot)", character);
        return;
      }

      // allow to use vehicle even if there is only min energy - to consume it and empty the fuel cell
      if (!VehicleEnergySystem.SharedHasEnergyCharge(vehicle, 1))
      //Math.Min(protoVehicle.EnergyUsePerSecondIdle,
      //         protoVehicle.EnergyUsePerSecondMoving)))
      {
        Logger.Info("Not enough energy in vehicle to enter it: " + vehicle, character);

        VehicleEnergyConsumptionSystem.ServerNotifyClientNotEnoughEnergy(character, protoVehicle);
        return;
      }

      var characterPrivateState = PlayerCharacter.GetPrivateState(character);
      characterPrivateState.CurrentActionState?.Cancel();

      var currentCharacterInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
      if (currentCharacterInteractionObject is not null)
      {
        InteractionCheckerSystem.SharedUnregister(character, currentCharacterInteractionObject, isAbort: true);
      }

      characterPublicState.ServerSetCurrentVehicle(vehicle);

      ServerResetLastVehicleMapMark(vehiclePrivateState);
      vehiclePublicState.PilotCharacter = character;

      vehiclePrivateState.ServerLastPilotCharacter = character;
      characterPrivateState.LastDismountedVehicleMapMark = default;

      Logger.Important("Player entered vehicle: " + vehicle, character);

      ServerGtaModTransferOwnership(vehicle, character);

      // rebuild physics
      character.ProtoWorldObject.SharedCreatePhysics(character);
      vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
      Server.World.SetPosition(character, vehicle.Position, writeToLog: false);

      protoVehicle.ServerSetUpdateRate(vehicle, isRare: false);
      protoVehicle.ServerOnCharacterEnterVehicle(vehicle, character);

      PlayerCharacter.SharedForceRefreshCurrentItem(character);

      // notify player and other players in scope
      using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(vehicle, tempPlayers);
      tempPlayers.Remove(character);

      Instance.CallClient(character,
                          _ => _.ClientRemote_OnVehicleEnterByCurrentPlayer(protoVehicle));
      Instance.CallClient(tempPlayers.AsList(),
                          _ => _.ClientRemote_OnVehicleEnterByOtherPlayer(vehicle.Position, protoVehicle));
    }

    public static void ServerGtaModTransferOwnership(IDynamicWorldObject vehicle, ICharacter character)
    {
      // GTA mod: automatically change the owner to the pilot or pilot's faction
      // (interacting with the vehicle is also enough to switch the ownership)
      var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
      var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
      var characterPublicState = character.GetPublicState<PlayerCharacterPublicState>();

      var hasNewOwner = false;

      if (vehiclePrivateState.Owners.Count != 1
          || vehiclePrivateState.Owners[0] != character.Name)
      {
        vehiclePrivateState.Owners.Clear();
        vehiclePrivateState.Owners.Add(character.Name);
        hasNewOwner = true;
      }

      if (vehiclePublicState.ClanTag != characterPublicState.ClanTag)
      {
        vehiclePublicState.ClanTag = characterPublicState.ClanTag;
        hasNewOwner = true;
      }

      if (hasNewOwner)
      {
        VehicleNamesSystem.VehicleNamesSystem.ServerGtaModNotifyVehicleOwnersAboutItsCurrentName(vehicle);
      }
    }

    private void ServerRemote_ExitVehicle(IDynamicWorldObject gameObjectVehicle)
    {
      var character = ServerRemoteContext.Character;
      ServerCharacterExitVehicle(character, gameObjectVehicle);
    }
  }
}