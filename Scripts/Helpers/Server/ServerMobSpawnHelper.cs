namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
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

  }
}