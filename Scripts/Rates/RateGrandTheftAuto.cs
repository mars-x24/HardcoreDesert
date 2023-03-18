using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateGrandTheftAuto
      : BaseRateBoolean<RateGrandTheftAuto>
  {
    [NotLocalizable]
    public override string Description =>
     @"Vehicle access without ownership check";

    public override string Id => "GrandTheftAuto";

    public override string Name => "GTA";

    public override RateVisibility Visibility => RateVisibility.Primary;

    public override bool ValueDefault => false;
  }
}