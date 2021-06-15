namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Helpers;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Extensions;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesServer;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class EventMigrationMutant : ProtoEventDrop
  {
    public static string NotificationUnderAttack = "Your base is under attack!";

    public override ushort AreaRadius => 30;

    public override string Description =>
        "Mutant lifeforms of this world seem to be enraged, protect your base.";

    public override TimeSpan EventDuration => TimeSpan.FromMinutes(15);

    public override double MinDistanceBetweenSpawnedObjects => 10;

    [NotLocalizable]
    public override string Name => "Migration (Mutant)";

    protected override double DelayHoursSinceWipe => 24 * EventConstants.ServerEventDelayMultiplier;

    protected override bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
    {
      return true;
    }

    protected override void ServerSpawnObjects(ILogicObject activeEvent, Vector2Ushort circlePosition, ushort circleRadius, List<IWorldObject> spawnedObjects)
    {
      var publicState = GetPublicState(activeEvent);

      int mobCount = 1;

      var world = Server.World;
      var tile = world.GetTile(publicState.AreaEventOriginalPosition);

      IStaticWorldObject claimObject = null;

      if (tile.StaticObjects.Count > 0)
      {
        claimObject = tile.StaticObjects[0];
        if(claimObject.ProtoGameObject is IProtoObjectLandClaim claim)
        {
          claim = claimObject.ProtoGameObject as IProtoObjectLandClaim;
          
          if (claim is ObjectLandClaimT1)
            mobCount = 1;
          else if (claim is ObjectLandClaimT2)
            mobCount = 4;
          else if (claim is ObjectLandClaimT3)
            mobCount = 8;
          else if (claim is ObjectLandClaimT4)
            mobCount = 13;
          else if (claim is ObjectLandClaimT5)
            mobCount = 20;
        }
      }

      List<IProtoWorldObject> list = new List<IProtoWorldObject>();
      
      for(int i = 0; i < mobCount; i++)
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
          var attempts = 2_000;
          do
          {
            var spawnPosition =
                SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
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

            if (!ServerCheckCanSpawn(protoObjectToSpawn, spawnPosition, tile.Height))
            {
              // doesn't match the tile requirements or inside a claimed land area
              continue;
            }

            if (attempts < 100 && Server.World.IsObservedByAnyPlayer(spawnPosition))
            {
              // observed by players
              continue;
            }

            var spawnedObject = ServerTrySpawn(protoObjectToSpawn, spawnPosition);
            spawnedObjects.Add(spawnedObject);
            Logger.Important($"Spawned world object: {spawnedObject} for active event {activeEvent}");
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
    }

    private static bool ServerCheckCanSpawn(IProtoWorldObject protoObjectToSpawn, Vector2Ushort spawnPosition, byte height)
    {
      return protoObjectToSpawn switch
      {
        IProtoCharacterMob
            => ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(
                   spawnPosition.ToVector2D(),
                   isPlayer: false)
               && !LandClaimSystem.SharedIsLandClaimedByAnyone(spawnPosition)
               && Api.Server.World.GetTile(spawnPosition).Height == height,

        IProtoStaticWorldObject protoStaticWorldObject
            // Please note: land claim check must be integrated in the object tile requirements
            => protoStaticWorldObject.CheckTileRequirements(
                spawnPosition,
                character: null,
                logErrors: false),

        _ => throw new ArgumentOutOfRangeException("Unknown object type to spawn: " + protoObjectToSpawn)
      };
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

    protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
    {
      int locationsCount = 5;

      if (Api.Server.Characters.OnlinePlayersCount >= 20)
        locationsCount = 10;

      if (Api.Server.Characters.OnlinePlayersCount >= 100)
        locationsCount = 20;

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
                                                              (from: TimeSpan.FromHours(0.5),
                                                               to: TimeSpan.FromHours(2.0)))
                       ));

      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantBoar>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantHyena>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedMutantWolf>());

      spawnPreset.Add(Api.GetProtoEntity<MobEnragedWildBoar>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedHyena>());
      spawnPreset.Add(Api.GetProtoEntity<MobEnragedWolf>());
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