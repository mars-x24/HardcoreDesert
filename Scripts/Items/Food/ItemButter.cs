namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemButter : ProtoItemFood
    {
        public override string Description => "A great ingredient for bakery and other foods, its made from fat and protein components of churned cream .";

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Butter";

        public override float StaminaRestore => -50;

        public override ushort OrganicValue => 40;
    }
}