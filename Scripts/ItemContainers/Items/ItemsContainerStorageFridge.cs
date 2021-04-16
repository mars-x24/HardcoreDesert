namespace AtomicTorch.CBND.CoreMod.ItemContainers.Items
{
  using AtomicTorch.CBND.CoreMod.ItemContainers.Items;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.GameApi.Data.Items;

  public class ItemsContainerStorageFridge : ItemsContainerStorage, IProtoItemsContainerFridge
  {
    public double SharedGetCurrentFoodFreshnessDecreaseCoefficient(IItemsContainer container)
    {
      var ownerObject = container.Owner;
      var protoFridge = (ProtoItemStorageFridge)ownerObject.ProtoGameObject;
      var publicState = ownerObject.GetPublicState<ItemStorageFridgePublicState>();
      var multiplier = protoFridge.GetCurrentFreshnessDurationMultiplier(publicState);

      if (multiplier <= 1)
      {
        // no change
        return 1;
      }

      return 1 / multiplier;
    }

  }
}