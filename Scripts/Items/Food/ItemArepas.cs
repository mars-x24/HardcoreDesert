namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemArepas : ProtoItemFood
    {
        public override string Description =>
            "Corn bread, mix of meats and veggies, a unique style for the classic arepas.";

        public override float FoodRestore => 20;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Arepas";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => 25;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectSavoryFood>(intensity: 0.20);
        }
    }
}