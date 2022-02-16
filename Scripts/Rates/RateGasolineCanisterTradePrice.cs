namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi;

    public class RateGasolineCanisterTradePrice
        : BaseRateUshort<RateGasolineCanisterTradePrice>
    {
        [NotLocalizable]
        public override string Description =>
          @"Price for gasoline in world trading station.";

        public override string Id => "GasolineCanisterTradePrice";

        public override string Name => "Gasoline canister trade price";

        public override ushort ValueDefault => 15;

        public override ushort ValueMax => 999;

        public override ushort ValueMin => 1;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}