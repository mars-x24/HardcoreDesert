using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  public class ItemStorageFridgePrivateState : ItemStoragePrivateState
  {
    [SyncToClient]
    public IItemsContainer ItemsEnergyContainer { get; set; }

  }
}