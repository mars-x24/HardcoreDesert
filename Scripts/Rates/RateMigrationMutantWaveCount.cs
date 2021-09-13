namespace AtomicTorch.CBND.CoreMod.Rates
{
  using AtomicTorch.CBND.GameApi;

  public class RateMigrationMutantWaveCount
      : BaseRateByte<RateMigrationMutantWaveCount>
  {
    [NotLocalizable]
    public override string Description => @"Number of waves for mutant migration event.";

    public override string Id => "MigrationMutant.WaveCount";

    public override string Name => "Mutant migration wave count";

    public override byte ValueDefault => 5;

    public override byte ValueMax => 50;

    public override byte ValueMaxReasonable => 10;

    public override byte ValueMin => 1;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Primary;
  }
}