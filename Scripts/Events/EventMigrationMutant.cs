namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Extensions;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesServer;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;

  public class EventMigrationMutant : ProtoEventDrop
  {
    public static string NotificationUnderAttack = "Your base is under attack!";

    public override ushort AreaRadius => 35;

    public override string Description =>
        "Mutant lifeforms of this world seem to be enraged, the estimated time for arrival is 5 minutes, protect your base!";

    public override double MinDistanceBetweenSpawnedObjects => 2;

    [NotLocalizable]
    public override string Name => "Migration (Mutant)";

    protected override double DelayHoursSinceWipe => 24 * EventConstants.ServerEventDelayMultiplier;

    public override TimeSpan EventDuration
    => this.EventDurationWithoutDelay + this.EventStartDelayDuration;

    public TimeSpan EventDurationWithoutDelay = TimeSpan.FromMinutes(15);

    public TimeSpan EventStartDelayDuration => TimeSpan.FromMinutes(5);

    // ReSharper disable once StaticMemberInGenericType
    private static readonly List<ICharacter> TempListPlayersInView = new();

    private static readonly IWorldServerService ServerWorld = IsServer
                                                              ? Server.World
                                                              : null;


    public double SharedGetTimeRemainsToEventStart(EventWithAreaPublicState publicState)
    {
      var time = IsServer
                     ? Server.Game.FrameTime
                     : Client.CurrentGame.ServerFrameTimeApproximated;
      var eventCreatedTime = publicState.EventEndTime - this.EventDuration.TotalSeconds;
      var timeSinceCreation = time - eventCreatedTime;
      var result = this.EventStartDelayDuration.TotalSeconds - timeSinceCreation;
      return Math.Max(result, 0);
    }

    protected override void ServerTryFinishEvent(ILogicObject activeEvent)
    {
      var publicState = activeEvent.GetPublicState<EventDropPublicState>();

      var serverTime = Server.Game.FrameTime;
      bool ended = (publicState.EventEndTime - serverTime) <= 0;

      var timeRemainsToEventStart = this.SharedGetTimeRemainsToEventStart(publicState);
      if (timeRemainsToEventStart != 0)
        return;

      var canFinish = true;

      var privateState = GetPrivateState(activeEvent);

      foreach (var spawnedObject in privateState.SpawnedWorldObjects)
      {
        if (!canFinish)
          break;

        if (spawnedObject.IsDestroyed)
          continue;

        if (spawnedObject is not ICharacter)
          continue;

        // should despawn
        // check that nobody is observing the mob
        var playersInView = TempListPlayersInView;
        playersInView.Clear();
        ServerWorld.GetCharactersInView((ICharacter)spawnedObject,
                                        playersInView,
                                        onlyPlayerCharacters: true);

        foreach (var playerCharacter in playersInView)
        {
          if (playerCharacter.ServerIsOnline && playerCharacter.ProtoCharacter is not PlayerCharacterSpectator)
          {
            // still has a spawned object which cannot be destroyed as it's observed by a player
            canFinish = false;
            break;
          }
        }
      }

      if (canFinish)
      {
        if (ended || publicState.NextWave > MigrantMutantConstants.MigrationMutantWaveCount)
        {
          // destroy after a second delay
          // to ensure the public state is synchronized with the clients
          ServerTimersSystem.AddAction(
            1,
            () =>
            {
              if (!activeEvent.IsDestroyed)
              {
                Server.World.DestroyObject(activeEvent);
              }
            });
        }
        else
        {
          if (publicState.CurrentWave != publicState.NextWave)
          {
            publicState.CurrentWave = publicState.NextWave;

            ServerTimersSystem.AddAction(
              delaySeconds: 10 + RandomHelper.Next(0, 5),
              () => this.ServerSpawnObjectsDelay(activeEvent, publicState.AreaCirclePosition, publicState.AreaCircleRadius, privateState.SpawnedWorldObjects));
          }
        }
      }
    }

    protected override bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
    {
      return true;
    }

    protected override void ServerSpawnObjects(ILogicObject activeEvent, Vector2Ushort circlePosition, ushort circleRadius, List<IWorldObject> spawnedObjects)
    {
      var publicState = activeEvent.GetPublicState<EventDropPublicState>();
      publicState.CurrentWave = publicState.NextWave = 1;

      ServerTimersSystem.AddAction(
        delaySeconds: this.EventStartDelayDuration.TotalSeconds + RandomHelper.Next(-60, 60),
        () => this.ServerSpawnObjectsDelay(activeEvent, circlePosition, circleRadius, spawnedObjects));
    }

    protected void ServerSpawnObjectsDelay(ILogicObject activeEvent, Vector2Ushort circlePosition, ushort circleRadius, List<IWorldObject> spawnedObjects)
    {
      var publicState = activeEvent.GetPublicState<EventDropPublicState>();

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
              mobCount = MigrantMutantConstants.MigrationMutantMobCount[0];
            }
            else if (claim is ObjectLandClaimT2)
            {
              mobCount = MigrantMutantConstants.MigrationMutantMobCount[1];
              tLevel = 2;
            }
            else if (claim is ObjectLandClaimT3)
            {
              mobCount = MigrantMutantConstants.MigrationMutantMobCount[2];
              tLevel = 3;
            }
            else if (claim is ObjectLandClaimT4)
            {
              mobCount = MigrantMutantConstants.MigrationMutantMobCount[3];
              tLevel = 4;
            }
            else if (claim is ObjectLandClaimT5)
            {
              mobCount = MigrantMutantConstants.MigrationMutantMobCount[4];
              tLevel = 5;
            }
          }
        }
      }

      List<IProtoWorldObject> list = new List<IProtoWorldObject>();

      if (publicState.CurrentWave == MigrantMutantConstants.MigrationMutantWaveCount)
      {
        var mob = this.GetLastWaveBossMob(tLevel);
        if (mob is not null)
          list.Add(mob);
      }

      if (publicState.CurrentWave >= MigrantMutantConstants.MigrationMutantWaveCount - 2)
      {
        var mob = this.GetWaveBossMob(tLevel);
        if (mob is not null)
          list.Add(mob);
      }

      if (publicState.CurrentWave > 1)
        mobCount += Convert.ToByte(Math.Ceiling(mobCount * publicState.CurrentWave * 0.1));

      for (int i = 0; i < mobCount; i++)
      {
        list.Add(this.SpawnPreset[RandomHelper.Next(0, this.SpawnPreset.Count)]);
      }

      var sqrMinDistanceBetweenSpawnedObjects =
          this.MinDistanceBetweenSpawnedObjects * this.MinDistanceBetweenSpawnedObjects;

      foreach (var protoObjectToSpawn in list)
      {
        TrySpawn();

        void TrySpawn()
        {
          var attempts = 500;

          do
          {
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

            var spawnedObject = ServerTrySpawn(protoObjectToSpawn as IProtoCharacterMob, spawnPosition);
            spawnedObjects.Add(spawnedObject);

            Logger.Important($"Spawned world object: {spawnedObject} for active event {activeEvent}");

            var mobPrivateState = spawnedObject.GetPrivateState<CharacterMobEnragedPrivateState>();
            if (mobPrivateState is not null)
              mobPrivateState.GoalTargetStructure = claimObject;

            break;
          }
          while (--attempts > 0);

          if (attempts == 0)
          {
            Logger.Error($"Cannot spawn world object: {protoObjectToSpawn} for active event {activeEvent}");
          }
        }
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
      EventDurationWithoutDelay = TimeSpan.FromMinutes(MigrantMutantConstants.MigrationMutantDurationWithoutDelay);

      int locationsCount = MigrantMutantConstants.MigrationMutantAttackNumber;

      if (Api.Server.Characters.OnlinePlayersCount >= 20)
        locationsCount *= 2;

      if (Api.Server.Characters.OnlinePlayersCount >= 100)
        locationsCount *= 4;


      if (SharedLocalServerHelper.IsLocalServer)
        locationsCount = 1;

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

          var position = claim.TilePosition;

          if (this.ServerCheckNoEventsNearby(position, AreaRadius, tempExistingEventsSameType.AsList()))
          {
            return position;
          }

        }
        while (--attempts > 0);
      }

      throw new Exception("Unable to pick an event position");
    }

    protected override void ServerPrepareDropEvent(Triggers triggers, List<IProtoWorldObject> spawnPreset)
    {
      triggers
          // trigger on time interval
          .Add(GetTrigger<TriggerTimeInterval>()
                   .Configure(
                           this.ServerGetIntervalForThisEvent(defaultInterval:
                                                              (from: TimeSpan.FromHours(1.0),
                                                               to: TimeSpan.FromHours(1.5)))
                       ));

      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantBoar>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantHyena>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantWolf>());

      spawnPreset.Add(Api.GetProtoEntity<MobEnragedWildBoar>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedHyena>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedWolf>());
    }

    private IProtoWorldObject GetLastWaveBossMob(int tLevel)
    {
      if (tLevel > 3)
        return Api.GetProtoEntity<MobEnragedLargePragmiumBear>();
      else
        return Api.GetProtoEntity<MobEnragedPragmiumBear>();
    }

    private IProtoWorldObject GetWaveBossMob(int tLevel)
    {
      if (tLevel > 3)
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