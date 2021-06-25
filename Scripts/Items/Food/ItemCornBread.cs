namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemCornBread : ProtoItemFood
    {
        public override string Description => "Corn flour made bread, sabroso!";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Corn bread";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => 25;
    }
}