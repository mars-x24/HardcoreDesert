using AtomicTorch.CBND.CoreMod.Items.Equipment;

namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  public interface IProtoItemBackpack : IProtoItemEquipmentDevice
  {
    byte SlotsCount { get; }
  }
}