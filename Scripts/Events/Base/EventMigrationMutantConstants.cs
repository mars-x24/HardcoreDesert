namespace AtomicTorch.CBND.CoreMod.Events
{
  using System;
  using System.Runtime.CompilerServices;
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;

  public static class MigrantMutantConstants
  {
    public static readonly int MigrationMutantWaveCount;

    public static readonly int[] MigrationMutantMobCount;

    public static readonly int MigrationMutantDurationWithoutDelay;

    static MigrantMutantConstants()
    {
      if (Api.IsClient)
      {
        return;
      }

      MigrationMutantDurationWithoutDelay = ServerRates.Get(
         "MigrationMutantDurationWithoutDelay",
         defaultValue: 15,
         @"Mutant migration duration in minutes without the 5 minutes delay.");

      MigrationMutantDurationWithoutDelay = MathHelper.Clamp(MigrationMutantDurationWithoutDelay, 1, 120);


      MigrationMutantWaveCount = ServerRates.Get(
          "MigrationMutantWaveCount",
          defaultValue: 5,
          @"Number of waves for mutant migration event.");

      MigrationMutantWaveCount = MathHelper.Clamp(MigrationMutantWaveCount, 0, 50);


      string mobCountString = ServerRates.Get(
      "MigrationMutantMobCount",
      defaultValue: "1,4,8,13,20",
      @"Number of mobs for each claims (T1 to T5).");

      string[] mobCountSplit = mobCountString.Replace(" ", "").Split(',');
      if (mobCountSplit.Length != 5)
        MigrationMutantMobCount = new int[] { 1, 4, 8, 13, 20 };
      else
        MigrationMutantMobCount = Array.ConvertAll(mobCountSplit, s => int.Parse(s));

      for (int i = 0; i < MigrationMutantMobCount.Length; i++)
        MigrationMutantMobCount[i] = MathHelper.Clamp(MigrationMutantMobCount[i], 0, 50);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void EnsureInitialized()
    {
    }
  }
}