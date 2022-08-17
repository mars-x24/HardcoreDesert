using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RatePvPShieldProtectionDuration
      : BaseRateInt<RatePvPShieldProtectionDuration>
  {
    [NotLocalizable]
    public override string Description =>
        @"Base S.H.I.E.L.D. duration (in seconds).";

    public override string Id => "PvP.ShieldProtection.Duration";

    public override string Name => "Base S.H.I.E.L.D. protection duration";

    public override int ValueDefault => 7 * 24 * 60 * 60; // 7 days

    public override int ValueMax => 7 * 24 * 60 * 60; // 7 days

    public override int ValueMin => 8 * 60 * 60; // 8 hours

    public override RateValueType ValueType => RateValueType.DurationSeconds;

    public override RateVisibility Visibility => RateVisibility.Advanced;
  }
}