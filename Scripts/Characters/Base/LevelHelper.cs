using AtomicTorch.CBND.CoreMod.Helpers.Server;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Helpers;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Characters
{
  public class LevelHelper
  {
    public static void SetLevel(IProtoCharacterMob character, CharacterMobPublicState publicState)
    {
      if (publicState is null)
        return;

      publicState.Level = GetLevel(character);

      //psMob.CurrentStats.ServerSetHealthMax(psMob.CurrentStats.HealthMax * GetLevelIncreaseFactor(psMob.Level));
    }

    public static int GetLevel(IProtoCharacterMob character)
    {
      if (character is null)
        return 1;

      if (character.IsBoss && Api.IsServer && (ServerLocalModeHelper.IsLocalServer || Api.IsEditor))
        return 1;

      return RandomLevels[RandomHelper.Next(0, RandomLevels.Count)];
    }

    public static float GetLevelIncreaseHealthPourcent(int level)
    {
      return GetHealthFactor(level) * 100.0f;
    }

    public static float GetHealthFactor(int level)
    {
      if (level <= 0)
        return 0;

      return level - 1;
    }

    private static List<int> RandomLevels = new List<int>() {
      1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
      2, 2, 2, 2, 2, 2, 2,
      3, 3, 3, 3,
      4, 4,
      5};

    public static float GetLevelIncreaseDamagePourcent(bool isBoss, int level)
    {
      return GetDamageFactor(isBoss, level) * 100.0f;
    }

    public static float GetDamageFactor(bool isBoss, int level)
    {
      if (isBoss || level <= 0)
        return 0;

      return (level - 1) * 0.125f;
    }
  }
}
