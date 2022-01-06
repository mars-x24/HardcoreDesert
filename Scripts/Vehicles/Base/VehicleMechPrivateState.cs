namespace AtomicTorch.CBND.CoreMod.Vehicles
{
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class VehicleMechPrivateState : VehiclePrivateState
  {
    [SyncToClient]
    public IItemsContainer EquipmentItemsContainer { get; set; }

    [SyncToClient]
    public IItemsContainer EquipmentItemsContainerBackup { get; set; }

    public IItemsContainer EquipmentItemsContainerTemp { get; set; }
  }
}