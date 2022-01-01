namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ItemStoragePrivateState : ItemPrivateState
  {
    [SyncToClient]
    public IItemsContainer ItemsContainer { get; set; }
  }
}