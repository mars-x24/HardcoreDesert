namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;

    public class ItemJelly : ProtoItemFood
    {
		
		public override string Description => "Tasty and juicy jelly.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Jelly";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 25;

        public override float StaminaRestore => 10;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHigh>(intensity: 0.10);
        }
    }
}