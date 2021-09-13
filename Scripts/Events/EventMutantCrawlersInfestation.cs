namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Data.Zones;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class EventMutantCrawlersInfestation
        : ProtoEvent<
            EventMutantCrawlersInfestation.PrivateState,
            EventPublicState,
            EmptyClientState>
  {
    public const string Description_FirearmsUseless
        = " Avoid roads or take the opportunity to obtain rare resources at your own risk.";

    public const string Description_Primary
        = "An infestation of mutated crawlers have emerged from under the asphalt.";

    private static Lazy<IReadOnlyList<IServerZone>> serverSpawnZones;

    public override bool ConsolidateNotifications => false;

    public override string Description
        => Description_Primary
           + "[br][br]"
           + Description_FirearmsUseless;

    public override TimeSpan EventDuration => TimeSpan.FromMinutes(10);

    [NotLocalizable]
    public override string Name => "Mutant crawlers infestation";

    public SpawnConfig SpawnScriptConfig { get; private set; }

    protected override double DelayHoursSinceWipe => 1 * RateWorldEventInitialDelayMultiplier.SharedValue;

    public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
    {
      if (trigger is not null
          && (this.ServerHasAnyEventOfType<IProtoEvent>()
              || ServerHasAnyEventOfTypeRunRecently<IProtoEvent>(TimeSpan.FromMinutes(45))))
      {
        // this event can run together or start soon after any other event
        return true;
      }

      if (this.ServerHasAnyEventOfType<EventMutantCrawlersInfestation>())
      {
        return false;
      }

      if (serverSpawnZones.Value.All(z => z.IsEmpty))
      {
        Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
        return false;
      }

      return true;
    }

    protected override void ServerInitializeEvent(ServerInitializeData data)
    {
      data.PrivateState.Init();
    }

    protected override void ServerOnEventDestroyed(ILogicObject activeEvent)
    {
      // destroy all the spawned objects
      foreach (var spawnedObject in GetPrivateState(activeEvent).SpawnedWorldObjects)
      {
        if (!spawnedObject.IsDestroyed)
        {
          Server.World.DestroyObject(spawnedObject);
        }
      }
    }

    protected override void ServerOnEventStarted(ILogicObject activeEvent)
    {
      this.ServerSpawnObjects(activeEvent,
                              GetPrivateState(activeEvent).SpawnedWorldObjects);
    }

    protected override void ServerPrepareEvent(Triggers triggers)
    {
      var intervalHours = RateWorldEventIntervalMutantCrawlersInfestation.SharedValueIntervalHours;
      triggers.Add(GetTrigger<TriggerTimeInterval>()
                       .Configure((intervalHours.From,
                                   intervalHours.To)));

      this.SpawnScriptConfig = Api.GetProtoEntity<SpawnEventMutantCrawlersInfestation>()
                                  .Configure(densityMultiplier: 2.0);
    }

    protected virtual async void ServerSpawnObjects(
        ILogicObject activeEvent,
        List<IWorldObject> spawnedObjects)
    {
      Logger.Important("Starting mobs spawn for " + activeEvent);

      var spawnScriptConfig = this.SpawnScriptConfig;
      var spawnScript = (ProtoZoneSpawnScript)spawnScriptConfig.ZoneScript;
      foreach (var zone in serverSpawnZones.Value)
      {
        await spawnScript.ServerInvoke(spawnScriptConfig, trigger: null, zone);
        var mobsTracker = SpawnedMobsTrackingManagersStore.Get(spawnScript, zone);
        spawnedObjects.AddRange(mobsTracker.EnumerateAll());
      }

      Logger.Important($"Finished mobs spawn for {activeEvent}: {spawnedObjects.Count} mobs spawned");
    }

    protected override void ServerTryFinishEvent(ILogicObject activeEvent)
    {
      var canFinish = true;

      var spawnedWorldObjects = GetPrivateState(activeEvent).SpawnedWorldObjects;
      var list = spawnedWorldObjects;
      for (var index = list.Count - 1; index >= 0; index--)
      {
        var spawnedObject = list[index];
        if (spawnedObject.IsDestroyed)
        {
          spawnedWorldObjects.RemoveAt(index);
          continue;
        }

        if (!Server.World.IsObservedByAnyPlayer(spawnedObject))
        {
          Server.World.DestroyObject(spawnedObject);
          spawnedWorldObjects.RemoveAt(index);
          continue;
        }

        // still has a spawned object which cannot be destroyed as it's observed by a player
        canFinish = false;
        break;
      }

      if (canFinish)
      {
        base.ServerTryFinishEvent(activeEvent);
      }
    }

    protected override void ServerWorldChangedHandler()
    {
      serverSpawnZones = new Lazy<IReadOnlyList<IServerZone>>(ServerSetupSpawnZones);
    }

    private static List<IServerZone> ServerSetupSpawnZones()
    {
      return new()
      {
        // during the event, the mutant crawlers appears in all road areas
        Api.GetProtoEntity<ZoneGenericRoads>().ServerZoneInstance
      };
    }

    public class PrivateState : BasePrivateState
    {
      public List<IWorldObject> SpawnedWorldObjects { get; }
          = new();

      public void Init()
      {
        for (var index = 0; index < this.SpawnedWorldObjects.Count; index++)
        {
          var worldObject = this.SpawnedWorldObjects[index];
          if (worldObject is null)
          {
            this.SpawnedWorldObjects.RemoveAt(index--);
          }
        }
      }
    }
  }
}