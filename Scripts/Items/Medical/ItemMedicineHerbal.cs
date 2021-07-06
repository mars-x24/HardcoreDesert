namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
  using System;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;

  public class ItemMedicineHerbal : ProtoItemMedical
  {
    public override double CooldownDuration => MedicineCooldownDuration.Short;

    public override string Description =>
        "Restores some health overtime and removes nausea and toxins. Probably doesn't have any side effects.";

    public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

    public override double MedicalToxicity => 0.3;

    public override string Name => "Herbal medicine";

    protected override void PrepareEffects(EffectActionsList effects)
    {
      effects
          .WillAddEffect<StatusEffectHealingSlow>(intensity: 0.40) // adds health regeneration
          .WillAddEffect<StatusEffectHealingFast>(intensity: 0.10) // 1 seconds (+10 HP each second)
          .WillRemoveEffect<StatusEffectNausea>()                 // removes nausea
          .WillRemoveEffect<StatusEffectToxins>(intensityToRemove: 0.1);
    }
  }
}