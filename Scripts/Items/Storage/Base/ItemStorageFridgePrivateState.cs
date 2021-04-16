namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ItemStorageFridgePrivateState : ItemStoragePrivateState
  {
    [SyncToClient]
    public IItemsContainer ItemsEnergyContainer { get; set; }

  }
}