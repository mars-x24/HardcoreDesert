using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateMigrationMutantAttackNumber
      : BaseRateByte<RateMigrationMutantAttackNumber>
  {
    [NotLocalizable]
    public override string Description => @"Number of base under attack for mutant migration event.";

    public override string Id => "MigrationMutant.AttackNumber";

    public override string Name => "Mutant migration attack number";

    public override byte ValueDefault => 3;

    public override byte ValueMax => 20;

    public override byte ValueMaxReasonable => 5;

    public override byte ValueMin => 0;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Primary;
  }
}