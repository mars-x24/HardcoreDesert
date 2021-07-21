namespace AtomicTorch.CBND.CoreMod.Events
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class MigrantMutantConstants
    {
        public static readonly double MigrantMutantWaveMultiplier;

        public static readonly double MigrantMutantMobsMultiplier;

        static MigrantMutantConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            MigrantMutantWaveMultiplier = ServerRates.Get(
                "MigrantMutantWaveMultiplier",
                defaultValue: 1.0,
                @"Migration (Mutant). Multiplier for the Waves.");

                MigrantMutantMobsMultiplier = ServerRates.Get(
                "MigrantMutantMobsMultiplier",
                defaultValue: 1.0,
                @"Migration (Mutant). Multiplier for the Mobs.");

            MigrantMutantWaveMultiplier = MathHelper.Clamp(MigrantMutantWaveMultiplier, 0, 10);
            MigrantMutantMobsMultiplier = MathHelper.Clamp(MigrantMutantMobsMultiplier, 0, 10);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void EnsureInitialized()
        {
        }
    }
}