using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateSeedTradePrice
        : BaseRateUshort<RateSeedTradePrice>
  {
    [NotLocalizable]
    public override string Description =>
      @"Price for seeds in world trading station.";

    public override string Id => "SeedTradePrice";

    public override string Name => "Seed trade price";

    public override ushort ValueDefault => 100;

    public override ushort ValueMax => 999;

    public override ushort ValueMin => 1;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Advanced;
  }
}