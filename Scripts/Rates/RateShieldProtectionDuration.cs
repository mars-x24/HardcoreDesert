using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateShieldProtectionDuration
      : BaseRateDouble<RateShieldProtectionDuration>
  {
    [NotLocalizable]
    public override string Description =>
        @"S.H.I.E.L.D. base protection duration (in hours).";

    public override string Id => "ShieldProtection.Duration";

    public override string Name => "Base S.H.I.E.L.D. protection duration (in hours)";

    public override double ValueDefault => 6 * 24; // 6 days;

    public override double ValueMax => 6 * 24; // 6 days;

    public override double ValueMin => 1 * 24; // 1 days;

    public override RateVisibility Visibility => RateVisibility.Advanced;

    public override RateValueType ValueType => RateValueType.Number;
  }
}