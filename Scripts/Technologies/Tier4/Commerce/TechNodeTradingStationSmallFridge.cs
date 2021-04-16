namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Commerce
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;

  public class TechNodeTradingStationSmallFridge : TechNode<TechGroupCommerceT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectTradingStationSmallFridge>();
    }
  }
}