namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Resources;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Extensions;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading.Tasks;

  public abstract class ProtoEventCrashSite
        : ProtoEventWithArea<
            EventCrashSitePrivateState,
            EventCrashSitePublicState,
            EmptyClientState>, IProtoEventDelayed
  {
    public const string ProgressTextFormat = "Progress: {0}/{1}";

    public override bool ConsolidateNotifications => true;

    public abstract double CrashSoundDelay { get; }

    public abstract double CrashShakeDelay { get; }

    public abstract double SpawnedObjectsDelay { get; }

    public double ShowEventAfter => 12;

    public double EventDurationSeconds => this.EventDuration.TotalSeconds;

    public abstract double MinDistanceBetweenSpawnedObjects { get; }

    public IProtoStaticWorldObject SpawnCrashPreset { get; private set; }

    public IProtoStaticWorldObject SpawnCrashPreset2 { get; private set; }

    public IReadOnlyList<IProtoWorldObject> SpawnPreset { get; private set; }

    public override string SharedGetProgressText(ILogicObject worldEvent)
    {
      var publicState = GetPublicState(worldEvent);
      if (!publicState.IsSpawnCompleted)
      {
        return null;
      }

      return string.Format(ProgressTextFormat,
                           publicState.ObjectsTotal - publicState.ObjectsRemains,
                           publicState.ObjectsTotal);
    }

    protected override void ServerInitializeEvent(ServerInitializeData data)
    {
      var privateState = data.PrivateState;
      var publicState = data.PublicState;

      privateState.Init();

      if (data.IsFirstTimeInit
          || publicState.IsSpawnCompleted)
      {
        return;
      }

      // the event was not properly initialized (spawn was not finished)
      // destroy the already spawned objects and restart the spawn
      foreach (var spawnedObject in privateState.SpawnedWorldObjects)
      {
        if (!spawnedObject.IsDestroyed)
          Server.World.DestroyObject(spawnedObject);
      }

      this.ServerSpawnObjectsDelayedAsync(data.GameObject,
                                   publicState.AreaCirclePosition,
                                   publicState.AreaCircleRadius);
    }

    protected override bool ServerIsValidEventPosition(Vector2Ushort tilePosition)
    {
      if (Server.World.IsObservedByAnyPlayer(tilePosition))
      {
        return false;
      }

      if (!ServerCheckCanSpawn(this.SpawnCrashPreset, tilePosition))
        return false;

      if (this.SpawnCrashPreset2 is not null && !ServerCheckCanSpawn(this.SpawnCrashPreset2, tilePosition))
        return false;

      foreach (var protoObjectToSpawn in this.SpawnPreset)
      {
        if (!ServerCheckCanSpawn(protoObjectToSpawn, tilePosition))
        {
          return false;
        }
      }

      return true;
    }

    protected bool CanCrashInCircle(Vector2Ushort tilePosition, ILogicObject activeEvent)
    {
      //Find a valid spot for main object
      int sizeX = Math.Max(this.SpawnCrashPreset.ViewBounds.MaxX, (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxX));
      int sizeY = Math.Max(this.SpawnCrashPreset.ViewBounds.MaxY, (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxY));
      int reduceRadius = Math.Max(sizeX, sizeY);
      int radius = this.AreaRadius - reduceRadius;

      bool isValidSpot;
      Vector2Ushort spawnPosition;

      var attemps = 200;
      do
      {
        isValidSpot = true;

        spawnPosition = SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
                 tilePosition,
                 (ushort)radius);

        //spawn on the left
        if (spawnPosition.X > tilePosition.X)
          isValidSpot = false;

        else if (!this.CanCrashOnTile(spawnPosition, activeEvent))
          isValidSpot = false;

        attemps--;
      }
      while (attemps > 0 && !isValidSpot);

      if (isValidSpot)
      {
        var privateState = activeEvent.GetPrivateState<EventCrashSitePrivateState>();
        privateState.SpawnedCrashObjectPosition = spawnPosition;
      }

      return isValidSpot;
    }

    protected bool CanCrashOnTile(Vector2Ushort spawnPosition, ILogicObject activeEvent)
    {
      //int sizeX = this.SpawnCrashPreset.ViewBounds.MaxX + (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxX);
      //int sizeY = this.SpawnCrashPreset.ViewBounds.MaxY + (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxY);
      int sizeX = Math.Max(this.SpawnCrashPreset.ViewBounds.MaxX, (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxX));
      int sizeY = Math.Max(this.SpawnCrashPreset.ViewBounds.MaxY, (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxY));

      int height = -1;

      for (int i = 0; i < sizeX; i++)
      {
        for (int j = 0; j < sizeY; j++)
        {
          Tile tile = Server.World.GetTile(spawnPosition.X + i, spawnPosition.Y + j);
          if (i == 0 && j == 0)
            height = tile.Height;

          if (!this.IsValidCrashTile(tile, height))
            return false;
        }
      }

      return true;
    }

    protected bool IsValidCrashTile(Tile tile, int height)
    {
      if (tile.Height != height)
        return false;

      if (tile.ProtoTile.Kind == TileKind.Water)
        return false;

      if (tile.IsCliffOrSlope || tile.IsOutOfBounds || !tile.IsValidTile)
        return false;

      foreach (var obj in tile.StaticObjects)
      {
        if (obj.ProtoGameObject is not IProtoObjectGatherable && obj.ProtoGameObject is not IProtoObjectMineral)
          return false;
      }

      return true;
    }

    protected void ClearCrashTiles(Vector2Ushort spawnPosition, ILogicObject activeEvent)
    {
      //int sizeX = this.SpawnCrashPreset.ViewBounds.MaxX + (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxX);
      //int sizeY = this.SpawnCrashPreset.ViewBounds.MaxY + (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxY);

      int sizeX = Math.Max(this.SpawnCrashPreset.ViewBounds.MaxX, (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxX));
      int sizeY = Math.Max(this.SpawnCrashPreset.ViewBounds.MaxY, (this.SpawnCrashPreset2 is null ? 0 : this.SpawnCrashPreset2.ViewBounds.MaxY));

      for (int i = 0; i < sizeX; i++)
      {
        for (int j = 0; j < sizeY; j++)
        {
          Tile tile = Server.World.GetTile(spawnPosition.X + i, spawnPosition.Y + j);

          var tempList = Api.Shared.GetTempList<IStaticWorldObject>();
          tempList.AddRange(tile.StaticObjects.ToList());

          foreach (var obj in tempList.AsList())
          {
            if (obj.ProtoGameObject is not IProtoObjectGatherable && obj.ProtoGameObject is not IProtoObjectMineral)
              continue;

            Server.World.DestroyObject(obj);
          }
        }
      }
    }

    protected abstract bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition);

    protected virtual void ServerOnCrashSiteEventStarted(ILogicObject worldEvent) { }

    protected override void ServerOnEventDestroyed(ILogicObject worldEvent)
    {
      var privateState = GetPrivateState(worldEvent);

      // destroy spawned crash object
      if (privateState.SpawnedCrashObject is not null && !privateState.SpawnedCrashObject.IsDestroyed)
        Server.World.DestroyObject(privateState.SpawnedCrashObject);

      // destroy all the spawned objects
      foreach (var spawnedObject in privateState.SpawnedWorldObjects)
      {
        if (!spawnedObject.IsDestroyed)
          Server.World.DestroyObject(spawnedObject);
      }
    }

    protected sealed override void ServerOnEventWithAreaStarted(ILogicObject worldEvent)
    {
      var publicState = GetPublicState(worldEvent);

      float maxDistance = Api.Server.World.WorldBounds.MaxX * 0.75f;

      var players = Api.Server.World.GetGameObjectsOfProto<ICharacter, PlayerCharacter>()
        .Where(c => c.ServerIsOnline)
        .Where(c => c.TilePosition.TileDistanceTo(publicState.AreaCirclePosition) <= maxDistance).ToList();

      ServerTimersSystem.AddAction(delaySeconds: this.CrashSoundDelay,
        () => this.CallClient(players, _ => _.ClientRemote_CrashSound(publicState.AreaCirclePosition.ToVector2D(), maxDistance)));

      ServerTimersSystem.AddAction(delaySeconds: this.CrashShakeDelay,
        () => this.CallClient(players, _ => _.ClientRemote_CrashShake()));

      ServerTimersSystem.AddAction(delaySeconds: this.SpawnedObjectsDelay,
           () => this.ServerSpawnObjectsDelayedAsync(worldEvent, publicState.AreaCirclePosition, publicState.AreaCircleRadius));
    }


    protected abstract void ServerPrepareCrashSiteEvent(Triggers triggers, List<IProtoWorldObject> listSpawnPreset, out IProtoWorldObject spawnCrashPreset, out IProtoWorldObject spawnCrashPreset2);

    protected sealed override void ServerPrepareEvent(Triggers triggers)
    {
      var listSpawnPreset = new List<IProtoWorldObject>();

      this.ServerPrepareCrashSiteEvent(triggers, listSpawnPreset, out IProtoWorldObject spawnCrashPreset, out IProtoWorldObject spawnCrashPreset2);

      Api.Assert(spawnCrashPreset is not null, "Spawn crash preset cannot be null");

      Api.Assert(listSpawnPreset.Count > 0, "Spawn preset cannot be empty");

      if (spawnCrashPreset is not IProtoStaticWorldObject)
        throw new Exception("Unknown object type for spawn crash object: " + spawnCrashPreset);

      if (spawnCrashPreset2 is not null && spawnCrashPreset2 is not IProtoStaticWorldObject)
        throw new Exception("Unknown object type for spawn crash object2: " + spawnCrashPreset);

      foreach (var protoWorldObject in listSpawnPreset)
      {
        switch (protoWorldObject)
        {
          case IProtoCharacterMob:
          case IProtoStaticWorldObject:
            // supported types
            continue;

          default:
            throw new Exception("Unknown object type in the spawn list: " + protoWorldObject);
        }
      }

      this.SpawnCrashPreset = (IProtoStaticWorldObject)spawnCrashPreset;
      this.SpawnCrashPreset2 = (IProtoStaticWorldObject)spawnCrashPreset2;

      this.SpawnPreset = listSpawnPreset;
    }

    protected override void ServerTryFinishEvent(ILogicObject worldEvent)
    {
      var canFinish = true;

      var privateState = GetPrivateState(worldEvent);

      List<IWorldObject> spawnedObjects = new List<IWorldObject>();
      spawnedObjects.AddRange(privateState.SpawnedWorldObjects);
      spawnedObjects.Add(privateState.SpawnedCrashObject);

      foreach (var spawnedObject in spawnedObjects)
      {
        if (spawnedObject is null || spawnedObject.IsDestroyed)
        {
          continue;
        }

        if (!Server.World.IsObservedByAnyPlayer(spawnedObject))
        {
          Server.World.DestroyObject(spawnedObject);
          continue;
        }

        // still has a spawned object which cannot be destroyed as it's observed by a player
        canFinish = false;
        break;
      }

      if (canFinish)
      {
        base.ServerTryFinishEvent(worldEvent);
      }
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      base.ServerUpdate(data);

      var publicState = data.PublicState;
      if (!publicState.IsSpawnCompleted)
      {
        return;
      }

      var worldEvent = data.GameObject;
      ServerRefreshEventState(worldEvent);

      if (publicState.ObjectsRemains == 0)
      {
        this.ServerTryFinishEvent(worldEvent);
      }
    }

    private static bool ServerCheckCanSpawn(IProtoWorldObject protoObjectToSpawn, Vector2Ushort spawnPosition)
    {
      return protoObjectToSpawn switch
      {
        IProtoCharacterMob
            => ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(
                   spawnPosition.ToVector2D(),
                   isPlayer: false)
               && !LandClaimSystem.SharedIsLandClaimedByAnyone(spawnPosition),

        IProtoStaticWorldObject protoStaticWorldObject
            // Please note: land claim check must be integrated in the object tile requirements
            => protoStaticWorldObject.CheckTileRequirements(
                spawnPosition,
                character: null,
                logErrors: false),

        _ => throw new ArgumentOutOfRangeException("Unknown object type to spawn: " + protoObjectToSpawn)
      };
    }

    private static void ServerRefreshEventState(ILogicObject worldEvent)
    {
      var privateState = GetPrivateState(worldEvent);
      var publicState = GetPublicState(worldEvent);

      var countDestroyed = 0;
      var totalCount = privateState.SpawnedWorldObjects.Count;

      foreach (var spawnedObject in privateState.SpawnedWorldObjects)
      {
        if (spawnedObject.IsDestroyed)
        {
          countDestroyed++;
        }
      }

      publicState.ObjectsRemains = (byte)Math.Min(byte.MaxValue, totalCount - countDestroyed);
      publicState.ObjectsTotal = (byte)Math.Min(byte.MaxValue, totalCount);
    }

    private static IWorldObject ServerTrySpawn(IProtoWorldObject protoObjectToSpawn, Vector2Ushort spawnPosition)
    {
      return protoObjectToSpawn switch
      {
        IProtoCharacterMob protoCharacterMob
            => Server.Characters.SpawnCharacter(protoCharacterMob,
                                                spawnPosition.ToVector2D()),

        IProtoStaticWorldObject protoStaticWorldObject
            => Server.World.CreateStaticWorldObject(
                protoStaticWorldObject,
                spawnPosition),

        _ => throw new Exception("Unknown object type to spawn: " + protoObjectToSpawn)
      };
    }

    private async void ServerSpawnObjectsDelayedAsync(
          ILogicObject worldEvent,
          Vector2Ushort circlePosition,
          ushort circleRadius)
    {
      var publicState = GetPublicState(worldEvent);
      Api.Assert(!publicState.IsSpawnCompleted, "Spawn already completed");

      var privateState = GetPrivateState(worldEvent);

      var spawnedObjects = privateState.SpawnedWorldObjects;


      //Spawn the crashed object
      if (this.CanCrashOnTile(privateState.SpawnedCrashObjectPosition, worldEvent))
      {
        this.ClearCrashTiles(privateState.SpawnedCrashObjectPosition, worldEvent);

        var crashedObject1 = Server.World.CreateStaticWorldObject(this.SpawnCrashPreset, privateState.SpawnedCrashObjectPosition);
        //if (crashedObject1 is not null)
        //  spawnedObjects.Add(crashedObject1);

        if (this.SpawnCrashPreset2 is not null)
        {
          var offset = privateState.SpawnedCrashObjectPosition; // + (this.SpawnCrashPreset.ViewBounds.MaxX, 0);
          var crashedObject2 = Server.World.CreateStaticWorldObject(this.SpawnCrashPreset2, offset); //.ToVector2Ushort());
          //if (crashedObject2 is not null)
          //  spawnedObjects.Add(crashedObject2);
        }
      }

      var sqrMinDistanceBetweenSpawnedObjects = this.MinDistanceBetweenSpawnedObjects
                                                * this.MinDistanceBetweenSpawnedObjects;

      var spawnedCount = 0;
      var yieldIfOutOfTime = (Func<Task>)Api.Server.Core.YieldIfOutOfTime;

      var protoObjectToSpawns = this.SpawnPreset;
      Logger.Important(
          $"Started async objects spawn for world event {worldEvent}: {this.SpawnPreset.Count} objects to spawn");
      var stopwatch = Stopwatch.StartNew();

      try
      {
        foreach (var protoObjectToSpawn in protoObjectToSpawns)
        {
          var attemptsRemains = 5000;
          do
          {
            await yieldIfOutOfTime();

            var spawnPosition = SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
                circlePosition,
                circleRadius);
            if (!this.ServerIsValidSpawnPosition(spawnPosition))
            {
              // doesn't match any specific checks determined by the inheritor (such as a zone test)
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

            if (!ServerCheckCanSpawn(protoObjectToSpawn, spawnPosition))
            {
              // doesn't match the tile requirements or inside a claimed land area
              continue;
            }

            if (Server.World.IsObservedByAnyPlayer(spawnPosition))
            {
              // observed by players
              continue;
            }

            var spawnedObject = ServerTrySpawn(protoObjectToSpawn, spawnPosition);
            spawnedObjects.Add(spawnedObject);
            spawnedCount++;
            Logger.Important($"Spawned world object: {spawnedObject} for world event {worldEvent}");
            break;
          }
          while (--attemptsRemains > 0);

          if (attemptsRemains == 0)
          {
            Logger.Error(
                $"Cannot spawn world object: {protoObjectToSpawn} for world event {worldEvent}");
          }
        }
      }
      finally
      {
        publicState.IsSpawnCompleted = true;
        Logger.Important(
            $"Completed async objects spawn for world event {worldEvent}: {spawnedCount}/{protoObjectToSpawns.Count} objects spawned. Total time: {stopwatch.ElapsedMilliseconds} ms");

        ServerRefreshEventState(worldEvent);

        this.ServerOnCrashSiteEventStarted(worldEvent);
      }
    }

    private void ClientRemote_CrashSound(Vector2D tilePosition, float maxDistance)
    {
      // play sound
      var crashSoundEmitter = Api.Client.Audio.PlayOneShot(SoundResourceCrashSite, tilePosition, volume: 1.5f);

      // crash sound distance
      crashSoundEmitter.CustomMinDistance = 0;
      crashSoundEmitter.CustomMaxDistance = maxDistance;

      crashSoundEmitter.Play();
    }

    private void ClientRemote_CrashShake()
    {
      this.ClientRemote_CrashShakeÌnterval(0.5, 5);
    }

    private void ClientRemote_CrashShakeÌnterval(double interval, int count)
    {
      if (count <= 0)
        return;

      // add screen shakes
      ClientComponentCameraScreenShakes.AddRandomShakes(
          duration: 1.0f,
          worldDistanceMin: 0.1f,
          worldDistanceMax: 0.5f);

      ClientTimersSystem.AddAction(interval, () => this.ClientRemote_CrashShakeÌnterval(interval, --count));
    }


    public static readonly SoundResource SoundResourceCrashSite = new("Events/CrashSite");
  }
}