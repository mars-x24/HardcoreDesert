namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  using AtomicTorch.CBND.CoreMod.Items.Equipment;

  public interface IProtoItemBackpack : IProtoItemEquipmentDevice
  {
    byte SlotsCount { get; }
  }
}