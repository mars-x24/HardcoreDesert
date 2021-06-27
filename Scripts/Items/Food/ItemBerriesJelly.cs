namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBerriesJelly : ProtoItemFood
    {
        public override string Description => "Sticky and tasty glowing fruit with a slight stimulant effect and high toxicity.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Jelly fruit";

        public override float FoodRestore => 2;

        public override float WaterRestore => 3;

        public override ushort OrganicValue => 8;

        public override float StaminaRestore => 10;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHigh>(intensity: 0.05) // adds high effect
                .WillAddEffect<StatusEffectToxins>(intensity: 0.15);
		}
		
		protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
		{
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}