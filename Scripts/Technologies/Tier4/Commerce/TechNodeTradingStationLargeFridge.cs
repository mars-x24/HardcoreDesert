using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Commerce
{
  public class TechNodeTradingStationLargeFridge : TechNode<TechGroupCommerceT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectTradingStationLargeFridge>();

      config.SetRequiredNode<TechNodeTradingStationSmallFridge>();
    }
  }
}