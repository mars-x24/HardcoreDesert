namespace AtomicTorch.CBND.CoreMod.Rates
{
  using AtomicTorch.CBND.GameApi;

  public class RateMigrationMutantAttackNumberLocalServer
      : BaseRateByte<RateMigrationMutantAttackNumberLocalServer>
  {
    [NotLocalizable]
    public override string Description => @"Number of base under attack for mutant migration event.";

    public override string Id => "MigrationMutant.AttackNumber.LocalServer";

    public override string Name => "Mutant migration attack number (Local server)";

    public override byte ValueDefault => 1;

    public override byte ValueMax => 10;

    public override byte ValueMaxReasonable => 3;

    public override byte ValueMin => 1;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Primary;
  }
}