namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
  using AtomicTorch.CBND.CoreMod.Systems.PvE;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Data.Zones;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class EventCrashSiteSpaceship : ProtoEventCrashSite
  {
    private static Lazy<IReadOnlyList<(IServerZone Zone, uint Weight)>> serverSpawnZones;

    public override ushort AreaRadius => PveSystem.ServerIsPvE
                                             ? (ushort)32
                                             : (ushort)32;

    public override string Description =>
        @"A spaceship with valuable cargo crashed in the area.
        [br]Rush in to collect unique loot!";

    public override TimeSpan EventDuration => TimeSpan.FromMinutes(30);

    public override double CrashSoundDelay => 0;

    public override double CrashShakeDelay => 9;

    public override double SpawnedObjectsDelay => 10;

    public override double MinDistanceBetweenSpawnedObjects => 10;

    [NotLocalizable]
    public override string Name => "Crash Site";

    protected override double DelayHoursSinceWipe => 2 * RateWorldEventInitialDelayMultiplier.SharedValue;

    public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
    {
      if (trigger is not null
          && (this.ServerHasAnyEventOfType<IProtoEvent>()
              || ServerHasAnyEventOfTypeRunRecently<IProtoEvent>(TimeSpan.FromMinutes(20))))
      {
        // this event cannot run together or start soon after any other event
        return false;
      }

      if (serverSpawnZones.Value.All(z => z.Zone.IsEmpty))
      {
        Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
        return false;
      }

      return true;
    }

    protected override bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
    {
      foreach (var serverZone in serverSpawnZones.Value)
      {
        if (serverZone.Zone.IsContainsPosition(spawnPosition))
        {
          return true;
        }
      }

      return false;
    }

    protected override void ServerOnCrashSiteEventStarted(ILogicObject worldEvent)
    {
      var publicState = GetPublicState(worldEvent);
      ServerEventLocationManager.AddUsedLocation(
          publicState.AreaCirclePosition,
          publicState.AreaCircleRadius * 1.2,
          duration: TimeSpan.FromHours(8));
    }


    protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
    {
      int locationsCount;
      if (PveSystem.ServerIsPvE)
      {
        locationsCount = 1;
      }
      else
      {
        locationsCount = Api.Server.Characters.OnlinePlayersCount >= 100 ? 1 : 1;
      }

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
          world.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(
              this));

      using var tempAllActiveEvents = Api.Shared.WrapInTempList(
          world.GetGameObjectsOfProto<ILogicObject, IProtoEventWithArea>());

      for (var globalAttempt = 0; globalAttempt < 10; globalAttempt++)
      {
        // try to select a zone which doesn't contain an active event of the same type
        var attempts = 25;
        IServerZone zoneInstance;
        do
        {
          zoneInstance = this.ServerSelectRandomZoneWithEvenDistribution(serverSpawnZones.Value);
          if (ServerCheckNoEventsInZone(zoneInstance, tempExistingEventsSameType.AsList()))
          {
            break;
          }

          zoneInstance = null;
        }
        while (--attempts > 0);

        zoneInstance ??= this.ServerSelectRandomZoneWithEvenDistribution(serverSpawnZones.Value)
                         ?? throw new Exception("Unable to pick an event position");

        // pick up a valid position inside the zone
        var maxAttempts = 2000;
        attempts = maxAttempts;
        do
        {
          var result = zoneInstance.GetRandomPosition(RandomHelper.Instance);
          if (this.ServerIsValidEventPosition(result)
              && !ServerEventLocationManager.IsLocationUsedRecently(
                  result,
                  this.AreaRadius * 4 * (attempts / (double)maxAttempts))
              && this.ServerCheckNoEventsNearby(
                  result,
                  this.AreaRadius * (1 + 3 * (attempts / (double)maxAttempts)),
                  tempAllActiveEvents.AsList()))
          {
            if(this.CanCrashInCircle(result, activeEvent))
              return result;
          }
        }
        while (--attempts > 0);
      }

      throw new Exception("Unable to pick an event position");
    }

    

    protected override void ServerPrepareCrashSiteEvent(
        Triggers triggers,
        List<IProtoWorldObject> spawnPreset, out IProtoWorldObject spawnPresetCrash, out IProtoWorldObject spawnPresetCrash2)
    {
      var intervalHours = RateWorldEventIntervalCrashSite.SharedValueIntervalHours;
      triggers.Add(GetTrigger<TriggerTimeInterval>()
                       .Configure((intervalHours.From,
                                   intervalHours.To)));

      int countOfEachObject = 7;

      for (int i = 0; i < countOfEachObject; i++)
      {
        spawnPreset.Add(Api.GetProtoEntity<ObjectLootPileGarbageSpaceship>());
        spawnPreset.Add(Api.GetProtoEntity<ObjectLootCrateSpaceship>());
      }

      spawnPresetCrash = Api.GetProtoEntity<ObjectCrashSiteSpaceship>();
      spawnPresetCrash2 = Api.GetProtoEntity<ObjectCrashSiteSpaceshipGround>();
    }

    protected override void ServerWorldChangedHandler()
    {
      serverSpawnZones = new Lazy<IReadOnlyList<(IServerZone, uint)>>(ServerSetupSpawnZones);
    }

    private static IReadOnlyList<(IServerZone, uint)> ServerSetupSpawnZones()
    {
      var result = new List<(IServerZone, uint)>();

      AddZone(Api.GetProtoEntity<ZoneBorealForest>());
      AddZone(Api.GetProtoEntity<ZoneBorealCoastLake>());
      AddZone(Api.GetProtoEntity<ZoneBorealCoastOcean>());
      AddZone(Api.GetProtoEntity<ZoneBorealMountain>());

      AddZone(Api.GetProtoEntity<ZoneTemperateForest>());
      AddZone(Api.GetProtoEntity<ZoneTemperateClay>());
      AddZone(Api.GetProtoEntity<ZoneTemperateCoastLake>());
      AddZone(Api.GetProtoEntity<ZoneTemperateCoastOcean>());
      AddZone(Api.GetProtoEntity<ZoneTemperateMeadow>());
      AddZone(Api.GetProtoEntity<ZoneTemperateSalt>());
      AddZone(Api.GetProtoEntity<ZoneTemperateBarren>());
      AddZone(Api.GetProtoEntity<ZoneTemperateSwamp>());

      AddZone(Api.GetProtoEntity<ZoneTropicalForest>());
      AddZone(Api.GetProtoEntity<ZoneTropicalMountain>());

      AddZone(Api.GetProtoEntity<ZoneGenericVolcanic>());
      AddZone(Api.GetProtoEntity<ZoneGenericRoads>());

      void AddZone(IProtoZone zone)
      {
        var instance = zone.ServerZoneInstance;
        result.Add((instance, (uint)instance.PositionsCount));
      }

      return result;
    }
  }
}