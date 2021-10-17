namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateAchievementsModdedServerEnabled
        : BaseRateBoolean<RateAchievementsModdedServerEnabled>
    {
        [NotLocalizable]
        public override string Description =>
            @"Achievement enabled in a modded server. Default to disabled like vanilla game.";

        public override string Id => "AchievementsModdedServerEnabled";

        public override string Name => "Achievements enabled (Modded server)";

        public override bool ValueDefault => false;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}