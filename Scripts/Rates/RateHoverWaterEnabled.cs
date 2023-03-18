using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateHoverWaterEnabled
      : BaseRateBoolean<RateHoverWaterEnabled>
  {
    [NotLocalizable]
    public override string Description =>
     @"Enable Hovering water board";

    public override string Id => "HoverWaterEnabled";

    public override string Name => "Hover water enabled";

    public override RateVisibility Visibility => RateVisibility.Advanced;

    public override bool ValueDefault => true;
  }
}