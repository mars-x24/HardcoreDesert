namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;

  public class ItemsContainerFridge : ItemsContainerDefault, IProtoItemsContainerFridge
  {
    public override bool CanAddItem(CanAddItemContext context)
    {
      var obj = context.Item.ProtoGameObject;
      var proto = context.Item.ProtoItem;

      if (obj is IProtoItemStorage)
        return false;

      if (!this.IsTradingStation(context.Container.Owner.ProtoGameObject))
      {
        if (obj is IProtoItemBackpack)
          return false;
      }

      return true;
    }

    private bool IsTradingStation(IProtoGameObject worldObject)
    {
      return worldObject is ObjectTradingStationSmallFridge || worldObject is ObjectTradingStationLargeFridge;
    }

    public double SharedGetCurrentFoodFreshnessDecreaseCoefficient(IItemsContainer container)
    {
      var ownerObject = container.OwnerAsStaticObject;
      var protoFridge = (IProtoObjectFridge)ownerObject.ProtoStaticWorldObject;
      var multiplier = protoFridge.ServerGetCurrentFreshnessDurationMultiplier(ownerObject);
      if (multiplier <= 1)
      {
        // no change
        return 1;
      }

      return 1 / multiplier;
    }
  }
}