namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.GameApi.Data.Items;

  public class ItemsContainerCharacterHotbar : ProtoItemsContainer
  {
    ///// <summary>
    ///// By default, add new items to the end of the container
    ///// </summary>
    //public override bool IsAddItemsToBeginning => false;

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