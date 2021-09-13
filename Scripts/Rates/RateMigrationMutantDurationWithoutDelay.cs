namespace AtomicTorch.CBND.CoreMod.Rates
{
  using AtomicTorch.CBND.GameApi;

  public class RateMigrationMutantDurationWithoutDelay
      : BaseRateByte<RateMigrationMutantDurationWithoutDelay>
  {
    [NotLocalizable]
    public override string Description => @"Mutant migration duration in minutes without the 5 minutes delay.";

    public override string Id => "MigrationMutant.DurationWithoutDelay";

    public override string Name => "Mutant migration duration";

    public override byte ValueDefault => 15;

    public override byte ValueMax => 120;

    public override byte ValueMaxReasonable => 30;

    public override byte ValueMin => 5;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Primary;
  }
}