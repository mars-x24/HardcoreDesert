namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using System.Collections.Generic;

  public class ItemRobotPrivateState : ItemWithDurabilityPrivateState
  {
    public IDynamicWorldObject WorldObjectRobot { get; set; }

    [SyncToClient]
    public bool RobotManufacturerInputEnabled { get; set; }

    [SyncToClient]
    public bool RobotManufacturerOutputEnabled { get; set; }

    [SyncToClient]
    public bool RobotManufacturerFuelEnabled { get; set; }

    [SyncToClient]
    public List<IProtoObjectManufacturer> AllowedStructure { get; set; }

    [SyncToClient]
    public ushort TimeRunIntervalSeconds { get; set; }

    [SyncToClient]
    public byte StructureLoadPercent { get; set; }
  }
}
