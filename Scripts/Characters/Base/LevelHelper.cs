using AtomicTorch.CBND.CoreMod.Helpers;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Zones;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Zones;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Characters
{
  public class LevelHelper
  {
    public static void SetLevel(IProtoCharacterMob protoCharacter, ICharacter character, CharacterMobPublicState publicState, CharacterMobPrivateState privateState)
    {
      if (publicState is null)
        return;

      int level;

      //check the zone
      int levelByZone = GetLevelByZone(character.TilePosition);
      if (levelByZone != 0)
      {
        level = levelByZone;
      }
      else
      {
        //check if there is a parent mob
        var list = character.PhysicsBody.PhysicsSpace.TestCircle(character.TilePosition.ToVector2D(), 10.0, CollisionGroups.Default).AsList()
        .Where(t => t.PhysicsBody.AssociatedWorldObject is not null)
        .Where(t => t.PhysicsBody.AssociatedWorldObject.ProtoWorldObject is IProtoCharacterBoss
                 || t.PhysicsBody.AssociatedWorldObject.ProtoWorldObject is IProtoCharacterSmallBoss).ToList();

        level = GetLevel(protoCharacter);

        if (list.Count > 0)
        {
          var mobPublicState = list[0].PhysicsBody.AssociatedWorldObject.GetPublicState<CharacterMobPublicState>();
          if (mobPublicState is not null && level > mobPublicState.Level)
            level = mobPublicState.Level;
        }
      }

      publicState.Level = level;
    }

    private static int GetLevel(IProtoCharacterMob character)
    {
      if (character is null)
        return 1;

      if (character.IsBoss && Api.IsServer && (SharedLocalServerHelper.IsLocalServer || Api.IsEditor))
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

      return (level - 1) * 0.25f;
    }

    private static int GetLevelByZone(Vector2Ushort tilePosition)
    {
      if (PositionInZone(tilePosition, Api.GetProtoEntity<ZoneMobLevel1>()))
        return 1;
      if (PositionInZone(tilePosition, Api.GetProtoEntity<ZoneMobLevel2>()))
        return 2;
      if (PositionInZone(tilePosition, Api.GetProtoEntity<ZoneMobLevel3>()))
        return 3;
      if (PositionInZone(tilePosition, Api.GetProtoEntity<ZoneMobLevel4>()))
        return 4;
      if (PositionInZone(tilePosition, Api.GetProtoEntity<ZoneMobLevel5>()))
        return 5;

      return 0;
    }

    private static bool PositionInZone(Vector2Ushort tilePosition, ProtoZoneDefault protoZone)
    {
      return protoZone.ServerZoneInstance.IsContainsPosition(tilePosition);
    }
  }
}
