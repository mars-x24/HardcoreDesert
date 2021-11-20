using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using AtomicTorch.GameEngine.Common.Helpers;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
  public partial class RobotSystem : ProtoSystem<RobotSystem>
  {
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




    public static void ClientSetRobotManufacturerSlotSetting(IItem itemRobot, bool isInputSlot, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerSettings(itemRobot, isInputSlot, value));
    }

    private void ServerRemote_SetRobotManufacturerSettings(IItem robot, bool isInputSlot, bool value)
    {
      var state = robot.GetPrivateState<ItemRobotPrivateState>();

      if (isInputSlot)
        state.RobotManufacturerInputEnabled = value;
      else
        state.RobotManufacturerOutputEnabled = value;
    }

    public static void ClientSetRobotManufacturerFuelSetting(IItem itemRobot, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerFuelSetting(itemRobot, value));
    }

    private void ServerRemote_SetRobotManufacturerFuelSetting(IItem robot, bool value)
    {
      var state = robot.GetPrivateState<ItemRobotPrivateState>();

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

      state.TimeRunIntervalSeconds = value == 0 ? (ushort)0 : MathHelper.Clamp(value, protoItem.DeliveryTimerSeconds, ushort.MaxValue);
    }

    public static void ClientSetRobotManufacturerLoadSetting(IItem itemRobot, byte value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerLoadSetting(itemRobot, value));
    }

    private void ServerRemote_SetRobotManufacturerLoadSetting(IItem robot, byte value)
    {
      var state = robot.GetPrivateState<ItemRobotPrivateState>();

      state.StructureLoadPercent = MathHelper.Clamp(value, (byte)1, (byte)100);
    }

    public static void ClientSetRobotManufacturerStructureSetting(IItem itemRobot, IProtoObjectManufacturer proto, bool value)
    {
      Instance.CallServer(_ => _.ServerRemote_SetRobotManufacturerStructureSetting(itemRobot, proto, value));
    }

    private void ServerRemote_SetRobotManufacturerStructureSetting(IItem robot, IProtoObjectManufacturer proto, bool value)
    {
      var state = robot.GetPrivateState<ItemRobotPrivateState>();

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

      state.AllowedStructure = temp;
    }
  }

}