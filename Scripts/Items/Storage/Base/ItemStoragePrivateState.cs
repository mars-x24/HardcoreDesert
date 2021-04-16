namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ItemStoragePrivateState : BasePrivateState
  {
    [SyncToClient]
    public IItemsContainer ItemsContainer { get; set; }
  }
}