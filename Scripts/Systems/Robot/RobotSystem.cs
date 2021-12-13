using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using AtomicTorch.GameEngine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HardcoreDesert.Scripts.Systems.Robot
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

      var owners = FindOwners(robotItem, robotObject);
      if (owners is null || owners.Count == 0)
        return;

      var itemHelper = FindBestItems(owners);

      //launch robot to target
      if (itemHelper is null || itemHelper.Target is null)
        return;

      //nothing to do
      if (!itemHelper.HasItems)
        return;

      if (!RobotTargetHelper.ServerStructureAllowed(itemHelper.Target, robotObject, privateStateItem))
        return;

      if (itemHelper.TargetItems.Count != 0 && !RobotTargetHelper.ServerPickupAllowed(itemHelper.TargetItems.Keys, robotObject))
        return;

      robotProto.ServerSetupAssociatedItem(robotObject, robotItem);

      robotProto.ServerStartRobot(robotObject, itemHelper.RobotOwner.Owner, itemHelper.RobotOwner.OwnerContainer);

      publicStateRobot.SetTargetPosition(itemHelper.Target, itemHelper.TargetItems, itemHelper.InputItems, itemHelper.FuelItems);

      if (!RobotTargetHelper.ServerTryRegisterCurrentPickup(publicStateRobot.TargetItems.Keys.ToList(), publicStateRobot.Target, robotObject, privateStateItem))
        publicStateRobot.ResetTargetPosition();
    }

    private static List<RobotOwner> FindOwners(IItem robotItem, IDynamicWorldObject robotObject)
    {
      var ownerContainer = robotItem.Container;
      if (ownerContainer is null)
        return null;

      if (ownerContainer.Owner is null)
        return null;

      List<RobotOwner> owners = null;

      if (ownerContainer.OwnerAsStaticObject is not null)
      {
        if (ownerContainer.OwnerAsStaticObject.ProtoGameObject is not ObjectGroundItemsContainer)  //nothing to do on ground       
        {
          RobotOwner robotOwner = new RobotOwner()
          {
            Owner = ownerContainer.OwnerAsStaticObject,
            Position = ownerContainer.OwnerAsStaticObject.TilePosition,
            OwnerContainer = ownerContainer,
            TargetItemProto = null,
            RobotItem = robotItem,
            RobotObject = robotObject
          };

          var publicState = robotOwner.Owner.GetPublicState<ObjectCratePublicState>();
          if (publicState is not null)
            robotOwner.TargetItemProto = publicState.IconSource;

          owners = new List<RobotOwner>() { robotOwner };
        }
      }
      else if (ownerContainer.OwnerAsCharacter is not null)
      {
        var characterPrivateState = ownerContainer.OwnerAsCharacter.GetPrivateState<PlayerCharacterPrivateState>();

        //must be in character inventory
        if (characterPrivateState.ContainerHand != ownerContainer && characterPrivateState.ContainerHotbar != ownerContainer)
        {
          RobotOwner robotOwner = new RobotOwner()
          {
            Owner = ownerContainer.OwnerAsCharacter,
            Position = ownerContainer.OwnerAsCharacter.TilePosition,
            OwnerContainer = ownerContainer,
            TargetItemProto = null,
            RobotItem = robotItem,
            RobotObject = robotObject
          };

          owners = new List<RobotOwner>() { robotOwner };
        }
      }
      else if (ownerContainer.Owner is ILogicObject logicObject)
      {
        var privateState = logicObject.GetPrivateState­­<LandClaimGroupPrivateState>();
        foreach (var areasGroup in privateState.ServerLandClaimAreasGroups)
        {
          if (areasGroup is null)
            continue;

          var privateStateAreasGroup = areasGroup.GetPrivateState<LandClaimAreasGroupPrivateState>();
          foreach (var area in privateStateAreasGroup.ServerLandClaimsAreas)
          {
            var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, false);

            using (var temp = Api.Shared.GetTempList<IStaticWorldObject>())
            {
              temp.AddRange(
                Api.Server.World.GetStaticWorldObjectsOfProtoInBounds<ProtoObjectGlobalChest>(bounds)
                  .Distinct());

              if (temp.Count > 0 && owners == null)
                owners = new List<RobotOwner>();

              foreach (var globalChest in temp.AsList())
              {
                RobotOwner robotOwner = new RobotOwner()
                {
                  Owner = globalChest,
                  Position = globalChest.TilePosition,
                  OwnerContainer = ownerContainer,
                  TargetItemProto = null,
                  RobotItem = robotItem,
                  RobotObject = robotObject
                };

                //var publicState = globalChest.GetPublicState<ObjectCratePublicState>();
                //if (publicState is not null)
                //  robotOwner.TargetItemProto = publicState.IconSource;

                owners.Add(robotOwner);
              }
            }
          }
        }
      }

      return owners;
    }

    private static RobotItemHelper FindBestItems(List<RobotOwner> owners)
    {
      RobotItemHelper goodItems = null;

      //foreach case is for Ender crate, it has more than one owner structure, look all areas around each structure
      foreach (var robotOwner in owners)
      {
        var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(robotOwner.Position);
        if (areasGroup is null)
          continue;

        var areas = areasGroup.GetPrivateState<LandClaimAreasGroupPrivateState>().ServerLandClaimsAreas;

        var privateStateItem = robotOwner.RobotItem.GetPrivateState<ItemRobotPrivateState>();

        if (!FindStructures(areas, robotOwner.RobotObject, robotOwner.Owner, privateStateItem, out List<IStaticWorldObject> outputManufacturer, out List<IStaticWorldObject> inputManufacturer))
          continue;

        var itemHelper = new RobotItemHelper(robotOwner, outputManufacturer, inputManufacturer);

        itemHelper.FindAllItems();

        //structure is inactive, go there first
        if (itemHelper.HasItems && !itemHelper.TargetIsActive)
          return itemHelper;

        if (itemHelper.HasItems)
          goodItems = itemHelper;
      }

      return goodItems;
    }

    private static bool FindStructures(List<ILogicObject> areas, IDynamicWorldObject robotObject, IWorldObject owner, ItemRobotPrivateState privateStateItem,
      out List<IStaticWorldObject> outputManufacturer, out List<IStaticWorldObject> inputManufacturer)
    {
      outputManufacturer = new List<IStaticWorldObject>();
      inputManufacturer = new List<IStaticWorldObject>();

      foreach (var area in areas)
      {
        if (owner is ICharacter && !LandClaimSystem.ServerIsOwnedArea(area, (ICharacter)owner, false))
          continue;

        var areaPrivateState = area.GetPrivateState<LandClaimAreaPrivateState>();
        if (!areaPrivateState.RobotManufacturerInputEnabled && !areaPrivateState.RobotManufacturerOutputEnabled)
          continue;

        if (!areaPrivateState.RobotManufacturerCharacterInventoryEnabled && owner is ICharacter)
          continue;

        if (!areaPrivateState.RobotManufacturerEnderCrateEnabled && owner.ProtoWorldObject is ObjectMassDriver)
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

      outputManufacturer = outputManufacturer.Distinct().ToList();
      inputManufacturer = inputManufacturer.Distinct().ToList();

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