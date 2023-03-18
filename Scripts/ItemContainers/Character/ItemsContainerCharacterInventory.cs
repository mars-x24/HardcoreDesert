using AtomicTorch.CBND.CoreMod.Items.Storage;
using AtomicTorch.CBND.GameApi.Data.Items;

namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
  public class ItemsContainerCharacterInventory : ProtoItemsContainer
  {
    public override bool CanAddItem(CanAddItemContext context)
    {
      //MOD
      if (!ProtoItemStorageFridge.SharedCanAddItem(context))
        return false;

      // allow everything
      return true;
    }
  }
}