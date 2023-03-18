using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
  public static class ServerMobSpawnHelper
  {
    public static void ServerTrySpawnMobsCustom(
        IProtoCharacter protoMob,
        int countToSpawn,
        RectangleInt excludeBounds,
        int maxSpawnDistanceFromExcludeBounds,
        double noObstaclesCheckRadius,
        int maxAttempts)
    {
      using var tempList = Api.Shared.GetTempList<ICharacter>();
      ServerTrySpawnMobsCustom(protoMob,
                               spawnedCollection: tempList.AsList(),
                               countToSpawn,
                               excludeBounds,
                               maxSpawnDistanceFromExcludeBounds,
                               noObstaclesCheckRadius,
                               maxAttempts);
    }

    public static void ServerTrySpawnMobsCustom(
        IProtoCharacter protoMob,
        ICollection<ICharacter> spawnedCollection,
        int countToSpawn,
        RectangleInt excludeBounds,
        int maxSpawnDistanceFromExcludeBounds,
        double noObstaclesCheckRadius,
        int maxAttempts)
    {
      if (countToSpawn <= 0)
      {
        return;
      }

      var spawnBounds = excludeBounds.Inflate(maxSpawnDistanceFromExcludeBounds,
                                              maxSpawnDistanceFromExcludeBounds);
      var physicsSpace = Api.Server.World.GetPhysicsSpace();

      while (maxAttempts-- > 0)
      {
        var position = new Vector2D(spawnBounds.Left + RandomHelper.NextDouble() * spawnBounds.Width,
                                    spawnBounds.Bottom + RandomHelper.NextDouble() * spawnBounds.Height);
        if (IsTooClose(position))
        {
          continue;
        }

        var character = ServerTrySpawnMob(position);
        if (character is null)
        {
          // cannot spawn there
          continue;
        }

        spawnedCollection.Add(character);

        countToSpawn--;
        if (countToSpawn == 0)
        {
          return;
        }
      }

      bool IsTooClose(in Vector2D position)
          => position.X >= excludeBounds.X
             && position.Y >= excludeBounds.Y
             && position.X < excludeBounds.X + excludeBounds.Width
             && position.Y < excludeBounds.Y + excludeBounds.Height;

      ICharacter ServerTrySpawnMob(Vector2D worldPosition)
      {
        foreach (var _ in physicsSpace.TestCircle(worldPosition,
                                                  radius: noObstaclesCheckRadius,
                                                  CollisionGroups.Default,
                                                  sendDebugEvent: false).EnumerateAndDispose())
        {
          // position is not valid for spawning
          return null;
        }

        if (LandClaimSystem.SharedIsLandClaimedByAnyone(worldPosition.ToVector2Ushort()))
        {
          // don't spawn mobs as the land is claimed
          return null;
        }


        if (!ServerCharacterSpawnHelper.IsValidSpawnTile(
                        Api.Server.World.GetTile(worldPosition.ToVector2Ushort()),
                        checkNeighborTiles: true))
        {
          return null;
        }

        return Api.Server.Characters.SpawnCharacter(protoMob,
                                                    worldPosition);
      }
    }


    public static void ServerTrySpawnMobs(IStaticWorldObject worldObject, List<ICharacter> mobsList, int mobSpawnDistance,
      int mobDespawnDistance, int mobsCountLimit, int serverSpawnMobsMaxCountPerIteration, IProtoCharacter protoMobToSpawn)
    {
      if (protoMobToSpawn is null)
        return;

      if (LandClaimSystem.SharedIsLandClaimedByAnyone(worldObject.Bounds))
      {
        // don't spawn mobs as the land is claimed
        return;
      }

      var mobsAlive = 0;
      for (var index = 0; index < mobsList.Count; index++)
      {
        var character = mobsList[index];
        if (character is null || character.IsDestroyed)
        {
          mobsList.RemoveAt(index--);
          continue;
        }

        if (character.TilePosition.TileSqrDistanceTo(worldObject.TilePosition)
            > mobDespawnDistance * mobDespawnDistance)
        {
          // the guardian mob is too far - probably lured away by a player
          using var tempListObservers = Api.Shared.GetTempList<ICharacter>();
          Api.Server.World.GetScopedByPlayers(character, tempListObservers);
          if (tempListObservers.Count == 0)
          {
            // despawn this mob as it's not observed by any player
            Api.Server.World.DestroyObject(character);
            mobsList.RemoveAt(index--);
          }

          continue;
        }

        mobsAlive++;
      }

      var countToSpawn = mobsCountLimit - mobsAlive;
      if (countToSpawn <= 0)
      {
        return;
      }

      // spawn mobs(s) nearby
      countToSpawn = Math.Min(countToSpawn, serverSpawnMobsMaxCountPerIteration);

      ServerMobSpawnHelper.ServerTrySpawnMobsCustom(protoMob: protoMobToSpawn,
                                                    spawnedCollection: mobsList,
                                                    countToSpawn,
                                                    excludeBounds: worldObject.Bounds.Inflate(4),
                                                    maxSpawnDistanceFromExcludeBounds: mobSpawnDistance,
                                                    noObstaclesCheckRadius: 1.0,
                                                    maxAttempts: 200);

      foreach (ICharacter mob in mobsList)
      {
        var privateState = mob.GetPrivateState<CharacterMobPrivateState>();
        privateState.IsAutoDespawnWithParent = true;
        privateState.ParentObject = worldObject;
      }
    }

    public static List<ICharacter> GetCloseEnragedMobs(Vector2Ushort tilePosition, byte radius)
    {
      List<ICharacter> list = null;

      using (var tempList = Api.Shared.GetTempList<ICharacter>())
      {
        Api.Server.World.GetCharactersInRadius(tilePosition, tempList, radius, false);

        list = tempList.AsList().Where(c => c.ProtoWorldObject is ProtoCharacterMobEnraged).ToList();
      }

      return list;
    }

    public static void ChangeEnragedMobsGoal(IStaticWorldObject worldObject, List<ICharacter> mobs, bool ifNoGoalOnly)
    {
      foreach (var mob in mobs)
      {
        var privateState = mob.GetPrivateState<CharacterMobEnragedPrivateState>();
        if (privateState is null)
          continue;

        if (!ifNoGoalOnly || (privateState.GoalTargetStructure is null || privateState.GoalTargetStructure.IsDestroyed))
        {
          privateState.GoalTargetStructure = worldObject;
          privateState.GoalTargetTimer = 0;
        }
      }
    }

    public static void ServerTrySpawnMobsEnraged(IStaticWorldObject goal, double MinDistanceBetweenSpawnedObjects, ushort circleRadius, int count = 1)
    {
      Tile centerTile = goal.OccupiedTile;
      Vector2Ushort circlePosition = centerTile.Position;

      var mobs = Api.FindProtoEntities<ProtoCharacterMobEnraged>();

      var mobsToSpawn = new List<ProtoCharacterMobEnraged>();

      for (int i = 0; i < count; i++)
        mobsToSpawn.Add(mobs[RandomHelper.Next(0, mobs.Count)]);

      var sqrMinDistanceBetweenSpawnedObjects = MinDistanceBetweenSpawnedObjects * MinDistanceBetweenSpawnedObjects;

      var physicsSpace = Api.Server.World.GetPhysicsSpace();

      var mobsSpawned = new List<IWorldObject>();

      foreach (var protoObjectToSpawn in mobsToSpawn)
      {
        var attempts = 5000;

        do
        {
          var spawnPosition =
              SharedSelectRandomOuterPositionInsideTheCircle(
                  circlePosition,
                  circleRadius);

          if (!ServerCheckCanSpawn(protoObjectToSpawn, spawnPosition, centerTile.Height))
          {
            // doesn't match the tile requirements or inside a claimed land area
            continue;
          }

          var isTooClose = false;
          foreach (var obj in mobsSpawned)
          {
            if (spawnPosition.TileSqrDistanceTo(obj.TilePosition)
                > sqrMinDistanceBetweenSpawnedObjects)
            {
              continue;
            }

            isTooClose = true;
            break;
          }

          if (isTooClose)
          {
            continue;
          }

          if (attempts < 100 && Api.Server.World.IsObservedByAnyPlayer(spawnPosition))
          {
            // observed by players
            continue;
          }

          //check for cliff
          bool hasCliff = false;
          var tempLineTestResults = physicsSpace.TestLine(spawnPosition.ToVector2D(), centerTile.Position.ToVector2D(), CollisionGroups.Default, false);
          foreach (var testResult in tempLineTestResults.AsList())
          {
            var worldObject = testResult.PhysicsBody.AssociatedWorldObject;
            if (testResult.PhysicsBody.AssociatedProtoTile != null)
            {
              if (testResult.PhysicsBody.AssociatedProtoTile.Kind != TileKind.Solid)
                continue;
              hasCliff = true;
              break;
            }
          }
          if (hasCliff)
            continue;

          var spawnedObject = Api.Server.Characters.SpawnCharacter(protoObjectToSpawn as IProtoCharacterMob, spawnPosition.ToVector2D());

          mobsSpawned.Add(spawnedObject);

          var mobPrivateState = spawnedObject.GetPrivateState<CharacterMobEnragedPrivateState>();
          if (mobPrivateState is not null)
            mobPrivateState.GoalTargetStructure = goal;

          var mobPublicState = spawnedObject.GetPublicState<CharacterMobPublicState>();

          //LevelHelper.RebuildLevel((ICharacter)spawnedObject, mobPublicState, mobPrivateState);

          break;
        }
        while (--attempts > 0);
      }
    }

    private static Vector2Ushort SharedSelectRandomOuterPositionInsideTheCircle(
                            Vector2Ushort circlePosition,
                            ushort circleRadius)
    {
      return SharedSelectRandomOuterPositionInsideTheCircle(
              circlePosition.ToVector2D(),
              circleRadius)
          .ToVector2Ushort();
    }

    private static Vector2D SharedSelectRandomOuterPositionInsideTheCircle(
                        Vector2D circlePosition,
                        double circleRadius)
    {
      var offset = circleRadius / 2.0 * RandomHelper.NextDouble() + circleRadius / 2.0;
      var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
      return new Vector2D(circlePosition.X + offset * Math.Cos(angle),
                          circlePosition.Y + offset * Math.Sin(angle));
    }

    private static bool ServerCheckCanSpawn(IProtoWorldObject protoObjectToSpawn, Vector2Ushort spawnPosition, byte height)
    {
      return !LandClaimSystem.SharedIsLandClaimedByAnyone(spawnPosition)
             && Api.Server.World.GetTile(spawnPosition).Height == height
             && ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(
                   spawnPosition.ToVector2D(),
                   isPlayer: false);
    }
  }
}