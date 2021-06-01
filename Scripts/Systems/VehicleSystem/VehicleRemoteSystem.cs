namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Items.Tools;
  using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
  using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
  using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
  using AtomicTorch.CBND.CoreMod.Systems.PvE;
  using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
  using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
  using AtomicTorch.CBND.CoreMod.Vehicles;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using AtomicTorch.GameEngine.Common.Extensions;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class VehicleRemoteSystem
        : ProtoActionSystem<
            VehicleRemoteSystem,
            ItemActionRequest,
            VehicleRemoteActionState,
            VehicleRemoteActionState.PublicState>
  {
    /// <summary>
    /// To prevent other players from taking vehicles (from world into garage) that were used recently
    /// by anyone else, a time delay applies (vehicles considered "in use" for this duration after pilot left).
    /// </summary>
    private const double ThresholdNoPilotSeconds = 5 * 60; // 5 minutes

    public override string Name => "Vehicle remote system";

    protected override ItemActionRequest ClientTryCreateRequest(ICharacter character)
    {
      var item = character.SharedGetPlayerSelectedHotbarItem();
      return new ItemActionRequest(character, item);
    }

    protected override void SharedOnActionCompletedInternal(VehicleRemoteActionState state, ICharacter character)
    {
      if (IsClient)
      {
        //Tile tile = Api.Client.World.TileAtCurrentMousePosition;
        Vector2D mousePosition = Api.Client.Input.MouseWorldPosition;

        this.CallServer(_ => _.ServerRemote_OnActionCompletedInternal(state.ItemVehicle, character, mousePosition));
      }
    }

    private void ServerRemote_OnActionCompletedInternal(IItem itemVehicleRemote, ICharacter character, Vector2D mousePosition)
    {
      if (!(itemVehicleRemote.ProtoItem is IProtoItemVehicleRemoteControl protoRemote))
        return;

      if (!CharacterEnergySystem.SharedHasEnergyCharge(character, protoRemote.EngeryUse))
        return;

      List<GarageVehicleEntry> list = VehicleGarageSystem.ServerGetCharacterVehicles(character, false);

      TakeVehicleResult result = TakeVehicleResult.Unknown;

      if (list.Count > 0)
      {
        var privateState = itemVehicleRemote.GetPrivateState<ItemVehicleRemoteControlPrivateState>();
        result = this.ServerTakeVehicle(privateState, character, mousePosition);
      }

      if (result == TakeVehicleResult.Success)
      {
        CharacterEnergySystem.ServerDeductEnergyCharge(character, protoRemote.EngeryUse);

        ItemDurabilitySystem.ServerModifyDurability(itemVehicleRemote, delta: -1);
      }
    }

    protected TakeVehicleResult ServerTakeVehicle(ItemVehicleRemoteControlPrivateState privateState, ICharacter character, Vector2D mousePosition)
    {
      var vehicle = Server.World.GetGameObjectById<IDynamicWorldObject>(GameObjectType.DynamicObject, privateState.VehicleID);

      if (vehicle is null)
      {
        Logger.Warning("Vehicle is not found", character);
        return TakeVehicleResult.Unknown;
      }

      if (!WorldObjectOwnersSystem.SharedIsOwner(character, vehicle))
      {
        Logger.Warning("Not an owner of the vehicle: " + vehicle, character);
        return TakeVehicleResult.NotOwner;
      }

      var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();

      var status = ServerGetVehicleStatus(vehicle, forCharacter: character);
      switch (status)
      {
        case VehicleStatus.InGarage:
          // allow to take
          break;

        case VehicleStatus.InWorld:
          //if (!PveSystem.ServerIsPvE)
          //{
          //  Logger.Warning("Cannot take a vehicle from world on a PvP server", character);
          //  return TakeVehicleResult.Unknown;
          //}

          // allow to take
          break;

        case VehicleStatus.InUse:
          return TakeVehicleResult.Error_InUse;

        case VehicleStatus.Docked:
          //return TakeVehicleResult.Error_Docked;
          // allow to take
          break;

        default:
          return TakeVehicleResult.Unknown;
      }

      var position = ServerGetVehicleSpawnPosition(mousePosition, character);
      if (!position.HasValue)
        return TakeVehicleResult.SpaceBlocked;


      var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();

      vehiclePrivateState.IsInGarage = false;
      vehiclePrivateState.ServerTimeSincePilotOffline = 0;
      vehiclePrivateState.ServerTimeSinceLastUse = ThresholdNoPilotSeconds + 1;

      Server.World.SetPosition(vehicle,
                               position.Value,
                               writeToLog: false);

      vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
      Logger.Important("Vehicle taken out of the garage: " + vehicle, character);

      // notify other players in scope

      using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(vehicle, tempPlayers);
      //tempPlayers.Remove(character);

      Instance.CallClient(tempPlayers.AsList(),
                          _ => _.ClientRemote_OnVehicleTakenFromGarageByOtherPlayer(vehicle, position.Value));

      return TakeVehicleResult.Success;
    }

    private void ClientRemote_OnVehicleTakenFromGarageByOtherPlayer(IDynamicWorldObject vehicle, Vector2D position)
    {
      ClientComponentTeleportationEffect.CreateEffect(vehicle, vehicle.TilePosition, 0.5, 0.0, false);

      Client.Audio.PlayOneShot(VehicleGarageSystem.SoundResourceTakeVehicle, position);
    }


    private static Vector2D? ServerGetVehicleSpawnPosition(Vector2D mousePosition, ICharacter character)
    {
      List<Tile> tiles = new List<Tile>();

      double distance = mousePosition.DistanceTo(character.TilePosition.ToVector2D());

      bool useMouse = false;
      if (distance <= 3)
      {
        useMouse = true;
        tiles.Add(Api.Server.World.GetTile(mousePosition.ToVector2Ushort()));
      }

      var neighborTiles = character.Tile.EightNeighborTiles.ToList();
      neighborTiles.Shuffle();

      tiles.AddRange(neighborTiles);

      foreach (var spawnTile in tiles)
      {
        if (spawnTile.Height != character.Tile.Height)
          continue;

        if (!spawnTile.IsValidTile || spawnTile.IsCliffOrSlope)
          continue;

        if (spawnTile.StaticObjects.Count != 0)
          continue;

        List<IDynamicWorldObject> objs = spawnTile.DynamicObjects.ToList<IDynamicWorldObject>();
        objs.Remove(character);

        if (objs.Count != 0)
          continue;

        if (spawnTile == tiles[0] && useMouse)
          return mousePosition;

        return spawnTile.Position.ToVector2D();
      }

      return null;
    }

    private static VehicleStatus ServerGetVehicleStatus(IDynamicWorldObject vehicle, ICharacter forCharacter)
    {
      var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
      if (vehiclePrivateState.IsInGarage)
      {
        return VehicleStatus.InGarage;
      }

      var publicState = vehicle.GetPublicState<VehiclePublicState>();
      if (publicState.PilotCharacter is not null)
      {
        return VehicleStatus.InUse;
      }

      var insideBay = false;

      foreach (var o in Server.World
                              .GetTile(vehicle.TilePosition)
                              .StaticObjects)
      {
        if (!(o.ProtoGameObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay))
        {
          continue;
        }

        var tempVehicles = Api.Shared.GetTempList<IDynamicWorldObject>();
        protoVehicleAssemblyBay.SharedGetVehiclesOnPlatform(o, tempVehicles);
        if (tempVehicles.Contains(vehicle))
        {
          insideBay = true;
          break;
        }
      }

      if (insideBay)
      {
        return VehicleStatus.Docked;
      }

      if (forCharacter == vehiclePrivateState.ServerLastPilotCharacter)
      {
        return VehicleStatus.InWorld;
      }

      return vehiclePrivateState.ServerTimeSinceLastUse < ThresholdNoPilotSeconds
                 ? VehicleStatus.InUse // was used recently
                 : VehicleStatus.InWorld;
    }


    protected override VehicleRemoteActionState SharedTryCreateState(ItemActionRequest request)
    {
      var character = request.Character;

      var item = request.Item;
      if (!(item?.ProtoGameObject is IProtoItemVehicleRemoteControl protoVehicleRemote))
        return null;

      //enough energy?

      var durationSeconds = protoVehicleRemote.ActionDuratioRecallSeconds;

      return new VehicleRemoteActionState(
          character,
          durationSeconds,
          item);
    }

    protected override void SharedValidateRequest(ItemActionRequest request)
    {

    }

    public static Task<IReadOnlyList<GarageVehicleEntry>> ClientGetVehiclesListAsync()
    {
      return Instance.CallServer(_ => _.ServerRemote_GetCharacterVehicles());
    }

    public IReadOnlyList<GarageVehicleEntry> ServerRemote_GetCharacterVehicles()
    {
      var character = ServerRemoteContext.Character;
      var result = new List<GarageVehicleEntry>();
      var allVehicles = Server.World.GetWorldObjectsOfProto<IProtoVehicle>();

      foreach (IDynamicWorldObject vehicle in allVehicles)
      {
        if (!WorldObjectOwnersSystem.SharedIsOwner(character, vehicle))
        {
          continue;
        }

        var vehicleStatus = ServerGetVehicleStatus(vehicle, forCharacter: character);
        result.Add(new GarageVehicleEntry(vehicle,
                                          vehicleStatus));
      }

      return result;
    }

    public bool ServerRemote_UpdateVehicleId(IItem item, uint vehicleId, bool cancel)
    {
      if (item is null)
        return false;

      var itemPrivateState = item.GetPrivateState<ItemVehicleRemoteControlPrivateState>();

      if (cancel)
      {
        itemPrivateState.VehicleID = 0;
        itemPrivateState.VehicleProto = null;
      }
      else
      {
        itemPrivateState.VehicleID = vehicleId;

        var vehicle = Server.World.GetGameObjectById<IDynamicWorldObject>(GameObjectType.DynamicObject, vehicleId);
        itemPrivateState.VehicleProto = vehicle.ProtoGameObject as IProtoVehicle;
      }

      return true;
    }
  }
}