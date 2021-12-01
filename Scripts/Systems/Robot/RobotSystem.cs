using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.GameApi.Data;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using HardcoreDesert.Scripts.Robots.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
  public partial class RobotSystem : ProtoSystem<RobotSystem>
  {
    public static void TryStartRobotFromContainer(IProtoItemRobot proto, IItem robotItem, ItemRobotPrivateState privateState, double deltaTime)
    {
      //look in which container the item is   
      if (robotItem.Container is null)
        return;

      var privateStateItem = robotItem.GetPrivateState<ItemRobotPrivateState>();
      var robotObject = privateStateItem.WorldObjectRobot;

      var privateStateRobot = robotObject.GetPrivateState<RobotPrivateState>();

      //wait for timer
      privateStateRobot.TimerInactive += deltaTime;

      if (privateState.TimeRunIntervalSeconds == 0)
        return;

      ushort timeBetweenRuns = Math.Max(privateState.TimeRunIntervalSeconds, proto.DeliveryTimerSeconds);

      if (privateStateRobot.TimerInactive < timeBetweenRuns)
        return;

      // I will set it back to 0 each time the robot calculate structures around him, this way server will save a lot of calculation
      //but the robot will stay longer in chest
      privateStateRobot.TimerInactive = 0;

      var robotProto = robotObject.ProtoGameObject as IProtoRobot;

      var publicStateRobot = robotObject.GetPublicState<RobotPublicState>();

      //low hp
      if (publicStateRobot.StructurePointsCurrent < 5.0f)
        return;

      if (!FindOwner(robotItem, out IWorldObject owner, out IItemsContainer ownerContainer, out Vector2Ushort position, out IProtoEntity targetItemProto))
        return;

      if (!FindStructures(robotObject, owner, privateStateItem, position, out List<IStaticWorldObject> outputManufacturer, out List<IStaticWorldObject> inputManufacturer))
        return;

      var itemHelper = new RobotItemHelper(robotObject, robotProto, robotItem.Container, targetItemProto, outputManufacturer, inputManufacturer);

      itemHelper.FindAllItems();

      //launch robot to target
      if (itemHelper.Target is null)
        return;

      //nothing to do
      if (itemHelper.InputItems.Count + itemHelper.FuelItems.Count + itemHelper.TargetItems.Count == 0)
        return;

      if (!RobotTargetHelper.ServerStructureAllowed(itemHelper.Target, robotObject, privateStateItem))
        return;

      if (itemHelper.TargetItems.Count != 0 && !RobotTargetHelper.ServerPickupAllowed(itemHelper.TargetItems.Keys, robotObject))
        return;

      robotProto.ServerSetupAssociatedItem(robotObject, robotItem);

      robotProto.ServerStartRobot(robotObject, owner, ownerContainer);

      publicStateRobot.SetTargetPosition(itemHelper.Target, itemHelper.TargetItems, itemHelper.InputItems, itemHelper.FuelItems);

      if (!RobotTargetHelper.ServerTryRegisterCurrentPickup(publicStateRobot.TargetItems.Keys.ToList(), publicStateRobot.Target, robotObject, privateStateItem))
        publicStateRobot.ResetTargetPosition();
    }

    private static bool FindOwner(IItem robotItem, out IWorldObject owner, out IItemsContainer ownerContainer, out Vector2Ushort position, out IProtoEntity targetItemProto)
    {
      owner = null;
      position = Vector2Ushort.Max;
      ownerContainer = robotItem.Container;
      targetItemProto = null;

      if (ownerContainer is null)
        return false;

      if (ownerContainer.OwnerAsStaticObject is not null)
      {
        if (ownerContainer.OwnerAsStaticObject.ProtoGameObject is not ObjectGroundItemsContainer)  //nothing to do on ground       
        {
          owner = ownerContainer.OwnerAsStaticObject;
          position = ownerContainer.OwnerAsStaticObject.TilePosition;

          var publicState = owner.GetPublicState<ObjectCratePublicState>();
          if (publicState is not null)
            targetItemProto = publicState.IconSource;
        }
      }
      else if (ownerContainer.OwnerAsCharacter is not null)
      {
        owner = ownerContainer.OwnerAsCharacter;
        position = ownerContainer.OwnerAsCharacter.TilePosition;

        var characterPrivateState = owner.GetPrivateState<PlayerCharacterPrivateState>();
        if (characterPrivateState.ContainerHand == ownerContainer ||
            characterPrivateState.ContainerHotbar == ownerContainer)
          owner = null; //must be in character inventory
      }
      else if (ownerContainer.Owner is LandClaimGroup)
      {
        //owner = ownerContainer.Owner;

      }

      if (owner is null)
        return false;

      return true;
    }

    private static bool FindStructures(IDynamicWorldObject robotObject, IWorldObject owner, ItemRobotPrivateState privateStateItem, Vector2Ushort position, out List<IStaticWorldObject> outputManufacturer, out List<IStaticWorldObject> inputManufacturer)
    {
      outputManufacturer = null;
      inputManufacturer = null;

      var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(position);
      if (areasGroup is null)
        return false;

      outputManufacturer = new List<IStaticWorldObject>();
      inputManufacturer = new List<IStaticWorldObject>();

      foreach (var area in areasGroup.GetPrivateState<LandClaimAreasGroupPrivateState>().ServerLandClaimsAreas)
      {
        if (owner is ICharacter)
          if (!LandClaimSystem.ServerIsOwnedArea(area, (ICharacter)owner, false))
            continue;

        var areaPrivateState = area.GetPrivateState<LandClaimAreaPrivateState>();
        if (!areaPrivateState.RobotManufacturerInputEnabled && !areaPrivateState.RobotManufacturerOutputEnabled)
          continue;

        if (!areaPrivateState.RobotManufacturerCharacterInventoryEnabled && owner is ICharacter)
          continue;

        var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, false);

        using (var temp = Api.Shared.GetTempList<IStaticWorldObject>())
        {
          temp.AddRange(
            Api.Server.World.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectManufacturer>(bounds)
              .Distinct()
              .Where(m => RobotTargetHelper.ServerStructureAllowed(m, robotObject, privateStateItem)));

          if (areaPrivateState.RobotManufacturerOutputEnabled)
            outputManufacturer.AddRange(temp.AsList().OrderBy(a => Guid.NewGuid()).ToList());

          if (areaPrivateState.RobotManufacturerInputEnabled)
            inputManufacturer.AddRange(temp.AsList().OrderBy(a => Guid.NewGuid()).ToList());
        }
      }

      if (outputManufacturer.Count == 0 && inputManufacturer.Count == 0)
        return false;

      return true;
    }

    #region Interface settings
    public static void ClientSetRobotManufacturerSettings(ILogicObject area, string playerName, bool isInputSlot, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerSettings(area, playerName, isInputSlot, value));
    }

    private void ServerRemote_SetRobotManufacturerSettings(ILogicObject area, string playerName, bool isInputSlot, bool value)
    {
      var state = area.GetPrivateState<LandClaimAreaPrivateState>();

      var landClaim = state.ServerLandClaimWorldObject;
      var proto = landClaim.ProtoGameObject as IProtoStaticWorldObject;

      if (!proto.SharedCanInteract(Server.Characters.GetPlayerCharacter(playerName), landClaim, false))
        return;

      if (isInputSlot)
        state.RobotManufacturerInputEnabled = value;
      else
        state.RobotManufacturerOutputEnabled = value;
    }

    public static void ClientSetRobotManufacturerCharacterInventorySetting(ILogicObject area, string playerName, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerCharacterInventorySetting(area, playerName, value));
    }

    private void ServerRemote_SetRobotManufacturerCharacterInventorySetting(ILogicObject area, string playerName, bool value)
    {
      var state = area.GetPrivateState<LandClaimAreaPrivateState>();

      var landClaim = state.ServerLandClaimWorldObject;
      var proto = landClaim.ProtoGameObject as IProtoStaticWorldObject;

      if (!proto.SharedCanInteract(Server.Characters.GetPlayerCharacter(playerName), landClaim, false))
        return;

      state.RobotManufacturerCharacterInventoryEnabled = value;
    }

    public static void ClientSetRobotManufacturerEnderCrateSetting(ILogicObject area, string playerName, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerEnderCrateSetting(area, playerName, value));
    }

    private void ServerRemote_SetRobotManufacturerEnderCrateSetting(ILogicObject area, string playerName, bool value)
    {
      var state = area.GetPrivateState<LandClaimAreaPrivateState>();

      var landClaim = state.ServerLandClaimWorldObject;
      var proto = landClaim.ProtoGameObject as IProtoStaticWorldObject;

      if (!proto.SharedCanInteract(Server.Characters.GetPlayerCharacter(playerName), landClaim, false))
        return;

      state.RobotManufacturerEnderCrateEnabled = value;
    }







    public static void ClientSetRobotManufacturerSlotSetting(IItem itemRobot, bool isInputSlot, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerSlotSetting(itemRobot, isInputSlot, value));
    }

    private void ServerRemote_SetRobotManufacturerSlotSetting(IItem itemRobot, bool isInputSlot, bool value)
    {
      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      this.CheckCharacterRobotPrivateState(itemRobot, state);
      if (isInputSlot)
        state.RobotManufacturerInputEnabled = value;
      else
        state.RobotManufacturerOutputEnabled = value;
    }

    public static void ClientSetRobotManufacturerFuelSetting(IItem itemRobot, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerFuelSetting(itemRobot, value));
    }

    private void ServerRemote_SetRobotManufacturerFuelSetting(IItem itemRobot, bool value)
    {
      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      this.CheckCharacterRobotPrivateState(itemRobot, state);
      state.RobotManufacturerFuelEnabled = value;
    }

    public static void ClientSetRobotManufacturerTimeRunSetting(IItem itemRobot, ushort value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerTimeRunSetting(itemRobot, value));
    }

    private void ServerRemote_SetRobotManufacturerTimeRunSetting(IItem itemRobot, ushort value)
    {
      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      var protoItem = itemRobot.ProtoGameObject as IProtoItemRobot;

      this.CheckCharacterRobotPrivateState(itemRobot, state);
      state.TimeRunIntervalSeconds = value == 0 ? (ushort)0 : MathHelper.Clamp(value, protoItem.DeliveryTimerSeconds, ushort.MaxValue);
    }

    public static void ClientSetRobotManufacturerLoadSetting(IItem itemRobot, byte value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerLoadSetting(itemRobot, value));
    }

    private void ServerRemote_SetRobotManufacturerLoadSetting(IItem itemRobot, byte value)
    {
      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      this.CheckCharacterRobotPrivateState(itemRobot, state);
      state.StructureLoadPercent = MathHelper.Clamp(value, (byte)1, (byte)100);
    }

    public static void ClientSetRobotManufacturerLoadInactiveOnlySetting(IItem itemRobot, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerLoadInactiveOnlySetting(itemRobot, value));
    }

    private void ServerRemote_SetRobotManufacturerLoadInactiveOnlySetting(IItem itemRobot, bool value)
    {
      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      this.CheckCharacterRobotPrivateState(itemRobot, state);
      state.LoadInactiveOnly = value;
    }




    public static void ClientSetRobotManufacturerStructureSetting(IItem itemRobot, IProtoObjectManufacturer proto, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerStructureSetting(itemRobot, proto, value));
    }

    private void ServerRemote_SetRobotManufacturerStructureSetting(IItem itemRobot, IProtoObjectManufacturer proto, bool value)
    {
      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      List<IProtoObjectManufacturer> temp = null;
      if (state.AllowedStructure != null)
        temp = new List<IProtoObjectManufacturer>(state.AllowedStructure);

      if (value)
      {
        if (temp is null)
          temp = new List<IProtoObjectManufacturer>();

        if (!temp.Contains(proto))
          temp.Add(proto);
      }
      else if (temp is not null)
      {
        if (temp.Contains(proto))
          temp.Remove(proto);
      }

      if (temp.Count == 0)
        temp = null;

      this.CheckCharacterRobotPrivateState(itemRobot, state);
      state.AllowedStructure = temp;
    }

    private void CheckCharacterRobotPrivateState(IItem robotItem, ItemRobotPrivateState state)
    {
      if (state.WorldObjectRobot is null)
        return;

      var stateRobot = state.WorldObjectRobot.GetPrivateState<RobotPrivateState>();
      if (stateRobot.TimerInactive >= state.TimeRunIntervalSeconds)
        stateRobot.TimerInactive = state.TimeRunIntervalSeconds - 5;
    }
    #endregion
  }

}