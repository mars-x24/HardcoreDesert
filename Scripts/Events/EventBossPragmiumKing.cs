namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.CoreMod.Systems.PvE;
  using AtomicTorch.CBND.CoreMod.Technologies;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Data.Zones;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Extensions;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  public class EventBossPragmiumKing : ProtoEventBoss
  {
    static EventBossPragmiumKing()
    {
      ServerEventDelayHours = ServerRates.Get(
          "EventDelay.BossPragmiumKing",
          defaultValue: 96.0,
          @"This hours value determines when the Pragmium King boss will start spawning for the first time.                  
                  Please note: for PvP server this value will be substituted by time-gating setting
                  for T4 specialized tech if it's larger than this value (as there is no viable way
                  for players to defeat the boss until T4 weapons becomes available).");
    }

    public override TimeSpan EventDurationWithoutDelay { get; }
        = TimeSpan.FromHours(1.5);

    public override TimeSpan EventStartDelayDuration { get; }
        = TimeSpan.FromMinutes(30);


    [NotLocalizable]
    public override string Name => "Pragmium King";

    private static double ServerEventDelayHours { get; }

    public override string Description => @"Pragmium King has appeared on the surface.
              [br]Valuable loot awaits the bravest survivors!";

    protected override double DelayHoursSinceWipe
    {
      get
      {
        var delayHours = 96.0; // 96 hours by default
        delayHours *= EventConstants.ServerEventDelayMultiplier;

        if (PveSystem.ServerIsPvE)
        {
          return delayHours;
        }

        // in PvP spawn boss not earlier than
        // T4 specialized tech (containing the necessary weapons) becomes available
        delayHours = Math.Max(
            delayHours,
            // convert seconds to hours
            TechConstants.PvpTechTimeGameTier4Specialized / 3600);

        return delayHours;
      }
    }

    public override bool ServerIsTriggerAllowedForBossEvent(ProtoTrigger trigger)
    {
      if (trigger is not null)
      {
        if (this.ServerHasAnyEventOfType<ProtoEventBoss>()
            || ServerHasAnyEventOfTypeRunRecently<ProtoEventBoss>(TimeSpan.FromHours(3)))
        {
          // another boss event is running now or run recently 
          return false;
        }
      }

      if (trigger is TriggerTimeInterval)
      {
        var delayHoursSinceWipe = ServerEventDelayHours;
        if (!PveSystem.ServerIsPvE)
        {
          // in PvP spawn Pragmium King not earlier than
          // T4 specialized tech (containing the necessary weapons) becomes available
          delayHoursSinceWipe = Math.Max(
              delayHoursSinceWipe,
              // convert seconds to hours
              TechConstants.PvpTechTimeGameTier4Specialized / 3600);
        }

        if (Server.Game.HoursSinceWorldCreation < delayHoursSinceWipe)
        {
          // too early
          return false;
        }
      }

      return true;
    }

    protected override IReadOnlyList<IServerZone> ServerSetupSpawnZones()
    {
      var result = new List<IServerZone>();
      if (!AddZone(Api.GetProtoEntity<ZoneEventFinalBoss>()))
        AddZone(Api.GetProtoEntity<ZoneEventBoss>());

      bool AddZone(IProtoZone zone)
      {
        var instance = zone.ServerZoneInstance;
        if (!instance.IsEmpty)
        {
          result.Add(instance);

          return true;
        }
        return false;
      }

      return result;
    }

    protected override void ServerPrepareBossEvent(
        Triggers triggers,
        List<IProtoSpawnableObject> spawnPreset)
    {
      triggers
          // trigger on time interval
          .Add(GetTrigger<TriggerTimeInterval>()
                   .Configure(
                           this.ServerGetIntervalForThisEvent(defaultInterval:
                                                              (from: TimeSpan.FromHours(12),
                                                               to: TimeSpan.FromHours(24)))
                       ));

      spawnPreset.Add(Api.GetProtoEntity<MobBossPragmiumKing>());
    }

    protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
    {
      var stopwatch = Stopwatch.StartNew();

      // select a random boss spawn zone
      var zoneInstance = this.ServerSpawnZones.Value.TakeByRandom();

      var chunkPositions = new HashSet<Vector2Ushort>(capacity: 50 * 50);
      var positionToCheck = new Stack<Vector2Ushort>();

      // perform the location selection several times to determine the possible locations
      var possibleLocations = new Dictionary<Vector2Ushort, uint>();
      for (var i = 0; i < 15; i++)
      {
        chunkPositions.Clear();
        positionToCheck.Clear();

        // select a random position inside the selected zone
        var randomPosition = zoneInstance.GetRandomPosition(RandomHelper.Instance);

        // use fill flood to locate all the positions
        // within the continuous area around the selected random position
        positionToCheck.Push(randomPosition);
        FillFlood();

        // calculate the center position of the area
        Vector2Ushort result = ((ushort)chunkPositions.Average(c => c.X),
                                (ushort)chunkPositions.Average(c => c.Y));

        if (!possibleLocations.ContainsKey(result))
        {
          possibleLocations.Add(result, 0);
        }

        possibleLocations[result]++;

        void FillFlood()
        {
          while (positionToCheck.Count > 0)
          {
            var pos = positionToCheck.Pop();
            if (pos.X == 0
                || pos.Y == 0
                || pos.X >= ushort.MaxValue
                || pos.Y >= ushort.MaxValue)
            {
              // reached the bound - not enclosed
              continue;
            }

            if (!chunkPositions.Add(pos))
            {
              // already visited
              continue;
            }

            if (!zoneInstance.IsContainsPosition(pos))
            {
              continue;
            }

            positionToCheck.Push(((ushort)(pos.X - 1), pos.Y));
            positionToCheck.Push(((ushort)(pos.X + 1), pos.Y));
            positionToCheck.Push((pos.X, (ushort)(pos.Y - 1)));
            positionToCheck.Push((pos.X, (ushort)(pos.Y + 1)));
          }
        }
      }

      //Logger.Dev("Possible boss spawn locations: "
      //           + possibleLocations.Select(p => p.Key + ": " + p.Value).GetJoinedString(Environment.NewLine));

      // each location has equal weight
      var selectedLocation = possibleLocations.Keys.TakeByRandom();
      Logger.Important(
          $"[Stopwatch] Selecting the boss event position took: {stopwatch.Elapsed.TotalMilliseconds:F1} ms");
      return selectedLocation;
    }

    protected override void ServerSpawnBossEventObjects(
           ILogicObject activeEvent,
           Vector2D circlePosition,
           ushort circleRadius,
           List<IWorldObject> spawnedObjects)
    {
      foreach (var protoObjectToSpawn in this.SpawnPreset)
      {
        TrySpawn();

        void TrySpawn()
        {
          const int maxAttempts = 3000;
          var attempt = 0;

          do
          {
            // select random position inside the circle
            // (the circle is expanded proportionally by the number of attempts performed)
            var spawnPosition =
                SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
                    circlePosition,
                    this.SpawnRadiusMax * (attempt / (double)maxAttempts));

            if (!this.ServerIsValidSpawnPosition(spawnPosition))
            {
              // doesn't match any specific checks determined by the inheritor (such as a zone test)
              continue;
            }

            var spawnedObject = Server.Characters.SpawnCharacter((IProtoCharacter)protoObjectToSpawn,
                                                                 spawnPosition);
            spawnedObjects.Add(spawnedObject);
            Logger.Important($"Spawned world object: {spawnedObject} for active event {activeEvent}");

            if (spawnedObject.ProtoGameObject is IProtoCharacterMob protoCharacterMob)
            {
              protoCharacterMob.ServerSetSpawnState(spawnedObject,
                                                    MobSpawnState.Spawning);
            }

            break;
          }
          while (++attempt < maxAttempts);

          if (attempt == maxAttempts)
          {
            Logger.Error($"Cannot spawn world object: {protoObjectToSpawn} for active event {activeEvent}");
          }
        }
      }

      this.DestroySalt(circlePosition, circleRadius);
    }

    protected void DestroySalt(Vector2D circlePosition, ushort circleRadius)
    {
      int size = circleRadius * 2;
      var rect = new RectangleInt((int)circlePosition.X - circleRadius, (int)circlePosition.Y - circleRadius, size, size);

      var list = Server.World.GetStaticWorldObjectsOfProtoInBounds<ObjectMineralSalt>(rect).ToList();

      foreach (var obj in list)
      {
        if (!obj.IsDestroyed)
        {
          Server.World.DestroyObject(obj);
        }
      }


    }


  }
}