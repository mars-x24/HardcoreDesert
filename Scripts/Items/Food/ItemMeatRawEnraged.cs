namespace AtomicTorch.CBND.CoreMod.Items.Food
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.Stats;
  using System;

  public class ItemMeatRawEnraged : ProtoItemFood
  {
    public override string Description =>
        "Raw enraged meat. Eating it raw is probably not a very good idea...";

    public override float FoodRestore => 10;

    public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

    public override bool IsAvailableInCompletionist => true;

    public override string Name => "Raw enraged meat";

    public override ushort OrganicValue => 20;

    public override float StaminaRestore => 10;

    public override float WaterRestore => -10;

    protected override void PrepareEffects(EffectActionsList effects)
    {
      effects
          .WillAddEffect<StatusEffectHealingFast>(intensity: 0.10) // 1 seconds (+10 HP each second)
          .WillAddEffect<StatusEffectRadiationPoisoning>(intensity: 0.05);
    }
  }
}