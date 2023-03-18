using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
  public class StatusEffectProtectionCold : ProtoStatusEffect
  {
    public override string Description =>
        "You are covered in a cold-resistant gel, significantly reducing any thermal damage you take from environmental sources.";

    public override double IntensityAutoDecreasePerSecondValue
        => 1.0 / 600.0; // total of 10 minutes for max possible time

    public override StatusEffectKind Kind => StatusEffectKind.Neutral;

    public override string Name => "Cold protection";

    protected override void PrepareEffects(Effects effects)
    {
      effects.AddPercent(this, StatName.ColdEffectMultiplier, -50);
    }
  }
}