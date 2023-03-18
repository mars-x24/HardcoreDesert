using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  public class ItemRobotPrivateState : ItemWithDurabilityPrivateState
  {
    public const byte DEFAULT_STRUCTURE_LOAD_PERCENT = 34;

    public IDynamicWorldObject WorldObjectRobot { get; set; }

    [SyncToClient]
    public bool RobotManufacturerInputEnabled { get; set; }

    [SyncToClient]
    public bool RobotManufacturerOutputEnabled { get; set; }

    [SyncToClient]
    public bool RobotManufacturerFuelEnabled { get; set; }

    [SyncToClient]
    public List<IProtoObjectStructure> AllowedStructures { get; set; }

    [SyncToClient]
    public List<IProtoObjectManufacturer> AllowedStructure { get; set; } //Old way

    [SyncToClient]
    public ushort TimeRunIntervalSeconds { get; set; }

    [SyncToClient]
    public byte StructureLoadPercent { get; set; }

    [SyncToClient]
    public bool LoadInactiveOnly { get; set; }
  }
}
