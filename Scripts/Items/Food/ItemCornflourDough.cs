namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemCornflourDough : ProtoItemFood
    {
        public override string Description => "Can be used in cooking for some specific recipes.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Cornflour Dough";

        public override ushort OrganicValue => 5;
    }
}