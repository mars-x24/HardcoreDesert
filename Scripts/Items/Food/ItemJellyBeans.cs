namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
	using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemJellyBeans : ProtoItemFood
    {
		//public override double CooldownDuration => MedicineCooldownDuration.None;
		
        public override string Description => "Yummy candy beans with energizing properties.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string Name => "Jelly beans";

        public override ushort OrganicValue => 30;

        public override float WaterRestore => -25;

        public override float StaminaRestore => 50;

        //public override double MedicalToxicity => 0.05;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHigh>(intensity: 0.05)
                .WillAddEffect<StatusEffectEnergyRush>(intensity: 0.04);
        }
    }
}