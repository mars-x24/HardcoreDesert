namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.State.NetSync;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Extensions;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesServer;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class EventMigrationMutant : ProtoEventWaveAttack
  {
    public static string NotificationUnderAttack = "Your base is under attack!";

    public override ushort AreaRadius => 35;

    public override string Description =>
        "Mutant lifeforms of this world seem to be enraged, the estimated time for arrival is 5 minutes, protect your base!";

    public override double MinDistanceBetweenSpawnedObjects => 2;

    [NotLocalizable]
    public override string Name => "Migration (Mutant)";

    protected override double DelayHoursSinceWipe => 24 * RateWorldEventInitialDelayMultiplier.SharedValue;

    protected override bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
    {
      return true;
    }

    protected override async void ServerSpawnObjectsAsync(ILogicObject worldEvent, Vector2Ushort circlePosition, ushort circleRadius)
    {
      var privateState = GetPrivateState(worldEvent);
      var spawnedObjects = privateState.SpawnedWorldObjects;

      var publicState = worldEvent.GetPublicState<EventWaveAttackPublicState>();

      int mobCount = 1;

      var world = Server.World;
      var tile = world.GetTile(publicState.AreaEventOriginalPosition);

      IStaticWorldObject claimObject = null;
      int tLevel = 1;

      if (tile.StaticObjects.Count > 0)
      {
        foreach (var o in tile.StaticObjects)
        {
          if (o.ProtoGameObject is IProtoObjectLandClaim claim)
          {
            claimObject = o;

            claim = claimObject.ProtoGameObject as IProtoObjectLandClaim;

            if (claim is ObjectLandClaimT1)
            {
              mobCount = RateMigrationMutantMobCount.SharedValues[0];
            }
            else if (claim is ObjectLandClaimT2)
            {
              mobCount = RateMigrationMutantMobCount.SharedValues[1];
              tLevel = 2;
            }
            else if (claim is ObjectLandClaimT3)
            {
              mobCount = RateMigrationMutantMobCount.SharedValues[2];
              tLevel = 3;
            }
            else if (claim is ObjectLandClaimT4)
            {
              mobCount = RateMigrationMutantMobCount.SharedValues[3];
              tLevel = 4;
            }
            else if (claim is ObjectLandClaimT5)
            {
              mobCount = RateMigrationMutantMobCount.SharedValues[4];
              tLevel = 5;
            }
          }
        }
      }

      List<IProtoWorldObject> protoObjectToSpawns = new List<IProtoWorldObject>();

      IProtoWorldObject bigBoss = this.GetLastWaveBossMob(tLevel);
      if (publicState.CurrentWave == RateMigrationMutantWaveCount.SharedValue)
      {
        if (bigBoss is not null)
          protoObjectToSpawns.Add(bigBoss);
      }

      IProtoWorldObject boss = this.GetWaveBossMob(tLevel);
      if (publicState.CurrentWave >= RateMigrationMutantWaveCount.SharedValue - 2)
      {
        if (boss is not null)
          protoObjectToSpawns.Add(boss);
      }

      if (publicState.CurrentWave > 1)
        mobCount += Convert.ToByte(Math.Ceiling(mobCount * publicState.CurrentWave * 0.1));

      for (int i = 0; i < mobCount; i++)
      {
        protoObjectToSpawns.Add(this.SpawnPreset[RandomHelper.Next(0, this.SpawnPreset.Count)]);
      }

      var sqrMinDistanceBetweenSpawnedObjects =
          this.MinDistanceBetweenSpawnedObjects * this.MinDistanceBetweenSpawnedObjects;

      var physicsSpace = Api.Server.World.GetPhysicsSpace();

      var yieldIfOutOfTime = (Func<Task>)Api.Server.Core.YieldIfOutOfTime;

      var spawnedCount = 0;

      try
      {
        foreach (var protoObjectToSpawn in protoObjectToSpawns)
        {
          var attempts = 5000;

          do
          {
            await yieldIfOutOfTime();

            var spawnPosition =
                SharedSelectRandomOuterPositionInsideTheCircle(
                    circlePosition,
                    circleRadius);

            if (!this.ServerIsValidSpawnPosition(spawnPosition))
            {
              // doesn't match any specific checks determined by the inheritor (such as a zone test)
              continue;
            }

            if (!ServerCheckCanSpawn(protoObjectToSpawn, spawnPosition, tile.Height))
            {
              // doesn't match the tile requirements or inside a claimed land area
              continue;
            }

            var isTooClose = false;
            foreach (var obj in spawnedObjects)
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

            if (attempts < 100 && Server.World.IsObservedByAnyPlayer(spawnPosition))
            {
              // observed by players
              continue;
            }


            //check for cliff
            bool hasCliff = false;
            var tempLineTestResults = physicsSpace.TestLine(spawnPosition.ToVector2D(), tile.Position.ToVector2D(), CollisionGroups.Default, false);
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

            var spawnedObject = ServerTrySpawn(protoObjectToSpawn as IProtoCharacterMob, spawnPosition);
            spawnedObjects.Add(spawnedObject);
            spawnedCount++;

            Logger.Important($"Spawned world object: {spawnedObject} for active event {worldEvent}");

            var mobPrivateState = spawnedObject.GetPrivateState<CharacterMobEnragedPrivateState>();
            if (mobPrivateState is not null)
              mobPrivateState.GoalTargetStructure = claimObject;

            var mobPublicState = spawnedObject.GetPublicState<CharacterMobPublicState>();
            ushort maxLevel = RateMigrationMutantMobMaxLevelPerWave.GetMaxLevelForWaveNumber((byte)(publicState.CurrentWave - 1));
            int level = -1; //random
            //if ((boss is not null && spawnedObject.ProtoGameObject.GetType() == boss.GetType()) ||
            //    (bigBoss is not null && spawnedObject.ProtoGameObject.GetType() == bigBoss.GetType()))
            //  level = publicState.CurrentWave;
            LevelHelper.RebuildLevel(level, maxLevel, (ICharacter)spawnedObject, mobPublicState, mobPrivateState);

            break;
          }
          while (--attempts > 0);

          if (attempts == 0)
          {
            Logger.Error($"Cannot spawn world object: {protoObjectToSpawn} for active event {worldEvent}");
          }
        }
      }
      finally
      {
        publicState.IsSpawnCompleted = true;
        Logger.Important(
            $"Completed async objects spawn for world event {worldEvent}: {spawnedCount}/{protoObjectToSpawns.Count} objects spawned");

        ServerRefreshEventState(worldEvent);
      }

      if (spawnedObjects.Count > 0 && claimObject is not null)
      {
        var claimPublicState = claimObject.GetPublicState<ObjectLandClaimPublicState>();
        var area = claimPublicState.LandClaimAreaObject;

        var areaPrivateState = LandClaimArea.GetPrivateState(area);

        var owners = areaPrivateState.ServerGetLandOwners();

        foreach (string owner in owners)
        {
          var character = Api.Server.Characters.GetPlayerCharacter(owner);
          if (character is not null)
          {
            NotificationSystem.ServerSendNotification(character,
              title: this.Name,
              message: NotificationUnderAttack,
              color: NotificationColor.Bad,
              autoHide: false);
          }
        }
      }

      publicState.NextWave++;
    }

    private Vector2Ushort SharedSelectRandomOuterPositionInsideTheCircle(
    Vector2Ushort circlePosition,
    ushort circleRadius)
    {
      return SharedSelectRandomOuterPositionInsideTheCircle(
              circlePosition.ToVector2D(),
              circleRadius)
          .ToVector2Ushort();
    }

    private Vector2D SharedSelectRandomOuterPositionInsideTheCircle(
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

    private static IWorldObject ServerTrySpawn(IProtoCharacterMob protoCharacterMob, Vector2Ushort spawnPosition)
    {
      return Server.Characters.SpawnCharacter(protoCharacterMob, spawnPosition.ToVector2D());
    }

    protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
    {
      EventDurationWithoutDelay = TimeSpan.FromMinutes(RateMigrationMutantDurationWithoutDelay.SharedValue);

      int locationsCount;

      //if (Api.Server.Characters.OnlinePlayersCount >= 20)
      //  locationsCount *= 2;

      //if (Api.Server.Characters.OnlinePlayersCount >= 100)
      //  locationsCount *= 4;

      if (SharedLocalServerHelper.IsLocalServer)
        locationsCount = RateMigrationMutantAttackNumberLocalServer.SharedValue;
      else
        locationsCount = RateMigrationMutantAttackNumber.SharedValue;

      for (var index = 0; index < locationsCount; index++)
      {
        if (!this.ServerCreateAndStartEventInstance())
        {
          break;
        }
      }
    }

    protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
    {
      var world = Server.World;
      using var tempExistingEventsSameType = Api.Shared.WrapInTempList(
          world.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(this));

      using var tempAllClaims = Api.Shared.WrapInTempList(
        world.GetGameObjectsOfProto<IStaticWorldObject, IProtoObjectLandClaim>());

      for (var globalAttempt = 0; globalAttempt < 10; globalAttempt++)
      {
        // pick up a valid position
        var maxAttempts = 100;
        int attempts = maxAttempts;
        do
        {

          var claim = tempAllClaims.AsList()[RandomHelper.Next(0, tempAllClaims.Count)];

          if (!IsLandClaimToBeDestroyed(claim))
          {
            var position = claim.TilePosition;

            if (this.ServerCheckNoEventsNearby(position, AreaRadius, tempExistingEventsSameType.AsList()))
            {
              var claimPublicState = claim.GetPublicState<ObjectLandClaimPublicState>();
              var area = claimPublicState.LandClaimAreaObject;
              var areaPrivateState = LandClaimArea.GetPrivateState(area);
              var publicState = activeEvent.GetPublicState<EventWaveAttackPublicState>();
              var owners = areaPrivateState.ServerGetLandOwners();

              publicState.BoundToPlayer = new NetworkSyncList<string>();

              foreach (string owner in owners)
              {
                publicState.BoundToPlayer.Add(owner);
              }

              return position;
            }
          }
        }
        while (--attempts > 0);
      }

      throw new Exception("Unable to pick an event position");
    }

    private static bool IsLandClaimToBeDestroyed(IStaticWorldObject worldObject)
    {
      if (worldObject.ProtoStaticWorldObject is ProtoObjectLandClaim)
      {
        var publicState = worldObject.GetPublicState<ObjectLandClaimPublicState>();
        if (publicState.ServerTimeForDestruction.HasValue)
          return true;
      }

      return false;
    }

    protected override void ServerPrepareSpawnPreset(Triggers triggers, List<IProtoWorldObject> spawnPreset)
    {

      var intervalHours = RateWorldEventIntervalMigrationMutant.SharedValueIntervalHours;
      triggers.Add(GetTrigger<TriggerTimeInterval>()
                       .Configure((intervalHours.From,
                                   intervalHours.To)));

      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantBoar>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantHyena>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantWolf>());

      spawnPreset.Add(Api.GetProtoEntity<MobEnragedWildBoar>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedHyena>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedWolf>());
    }

    private IProtoWorldObject GetLastWaveBossMob(int tLevel)
    {
      if (tLevel >= 5)
        return Api.GetProtoEntity<MobEnragedLargePragmiumBear>();
      else if (tLevel >= 3)
        return Api.GetProtoEntity<MobEnragedPragmiumBear>();
      else
        return null;
    }

    private IProtoWorldObject GetWaveBossMob(int tLevel)
    {
      if (tLevel >= 5)
        return Api.GetProtoEntity<MobEnragedPragmiumBear>();
      else
        return null;
    }

    protected override bool ServerCreateEventSearchArea(
    IWorldServerService world,
    Vector2Ushort eventPosition,
    ushort circleRadius,
    out Vector2Ushort circlePosition)
    {
      circlePosition = eventPosition;
      return true;
    }
  }
}