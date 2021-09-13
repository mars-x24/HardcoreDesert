namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi;

    public class RateGlobalStorageCapacity
        : BaseRateByte<RateGlobalStorageCapacity>
    {
        [NotLocalizable]
        public override string Description =>
          @"How many global storage slots are allowed per base.
                          The value should be within 0-128 range.";

        public override string Id => "GlobalStorageCapacity";

        public override string Name => "Global storage capacity.";

        public override byte ValueDefault => 64;

        public override byte ValueMax => 128;

        public override byte ValueMin => 0;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}