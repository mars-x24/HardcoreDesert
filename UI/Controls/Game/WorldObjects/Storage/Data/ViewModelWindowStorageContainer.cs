namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage.Data
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;

  public class ViewModelWindowStorageContainer : BaseViewModel
  {
    public ViewModelWindowStorageContainer(IItem itemStorage)
    {
      this.ItemStorage = itemStorage;

      var privateState = itemStorage.GetPrivateState<ItemStoragePrivateState>();

      this.ViewModelItemsContainerExchange = new ViewModelItemsContainerExchange(privateState.ItemsContainer)
      {
        IsContainerTitleVisible = false
      };
    }

    public ViewModelItemsContainerExchange ViewModelItemsContainerExchange { get; }

    public IItem ItemStorage { get; }

  }
}