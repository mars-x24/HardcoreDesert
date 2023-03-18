using System.ComponentModel;

namespace AtomicTorch.CBND.CoreMod.Vehicles
{
  public enum VehicleWeaponHardpoint : byte
  {
    [Description("Normal")]
    Normal,

    [Description("Large")]
    Large,

    [Description("Tool")]
    Tool,

    [Description("Exotic")]
    Exotic
  }
}