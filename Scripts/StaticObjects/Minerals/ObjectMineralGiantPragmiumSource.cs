namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.CoreMod.Items.Weapons;
  using AtomicTorch.CBND.CoreMod.Objects;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Special;
  using AtomicTorch.CBND.CoreMod.Systems.Creative;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.CoreMod.Systems.PvE;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Extensions;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using AtomicTorch.CBND.GameApi.ServicesServer;
  using AtomicTorch.GameEngine.Common.Extensions;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class ObjectMineralGiantPragmiumSource
      : ProtoObjectMineral
        <ObjectMineralGiantPragmiumSource.PrivateState,
            ObjectMineralGiantPragmiumSource.PublicState,
            DefaultMineralClientState>,
        IProtoObjectPsiSource
  {
    public const string ErrorCannotBuild_PragmiumSourceTooCloseOnPvE =
        "Too close to a pragmium source.";

    // How many nodes the game server should spawn when pragmium Source is destroyed.
    private const int DestroySpawnNodeCount = 27;

    // How many guardian mobs pragmium source can have simultaneously.
    private const int MobsCountLimit = 10;

    // How many guardian mobs a pragmium source will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
    private const int ServerSpawnMobsMaxCountPerIteration = 5; // spawn at max 2 mobs per iteration

    // How many nodes a pragmium source will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
    private const int ServerSpawnNodesMaxCountPerIteration = 5; // spawn at max 1 nodes per iteration


    private const int MobSpawnDistance = 6;

    private const int MobDespawnDistance = 8;

    // How many nodes a pragmium source can have simultaneously.
    private const int NodesCountLimit = 30;


    public static readonly ConstructionTileRequirements.Validator ValidatorCheckNoPragmiumSourceNearbyOnPvE
        = new(ErrorCannotBuild_PragmiumSourceTooCloseOnPvE,
              context =>
              {
                var forCharacter = context.CharacterBuilder;
                if (forCharacter is null)
                {
                  return true;
                }

                if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                {
                  // this limitation doesn't apply to PvP servers
                  return true;
                }

                if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                {
                  return true;
                }

                var position = context.Tile.Position;
                var world = IsServer
                                    ? (IWorldService)Server.World
                                    : (IWorldService)Client.World;

                var pragmiumSources = world.GetStaticWorldObjectsOfProto<ObjectMineralGiantPragmiumSource>();
                var maxDistanceSqr = 5 + LandClaimSystem.MaxLandClaimSize.Value / 2;
                maxDistanceSqr *= maxDistanceSqr;

                foreach (var objectPragmiumSource in pragmiumSources)
                {
                  if (position.TileSqrDistanceTo(objectPragmiumSource.TilePosition)
                          <= maxDistanceSqr)
                  {
                    // too close to a pragmium source
                    return false;
                  }
                }

                return true;
              });

    private static readonly Lazy<IProtoCharacter> LazyProtoMob
        = new(GetProtoEntity<MobPragmiumBeetle>);

    private static readonly Lazy<ObjectMineralPragmiumNode> ProtoNodeLazy
        = new(GetProtoEntity<ObjectMineralPragmiumNode>);

    public override bool IsAllowDroneMining => false;

    // we don't want to see any decals under it
    public override StaticObjectKind Kind => StaticObjectKind.Structure;

    public override string Name => "Giant pragmium source";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

    public override ReadOnlySoundPreset<ObjectMaterial> OverrideSoundPresetHit
        => MaterialHitsSoundPresets.OverridePragmium;

    public double PsiIntensity => 1.0;

    public double PsiRadiusMax => 20;

    public double PsiRadiusMin => 10;

    public override float StructurePointsMax => 50000;

    // has light source
    public override BoundsInt ViewBoundsExpansion => new(minX: -12,
                                                         minY: -4,
                                                         maxX: 12,
                                                         maxY: 8);

    public bool ServerIsPsiSourceActive(IWorldObject worldObject)
    {
      return true;
    }

    public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
    {
      return new(2, 4.4);
    }

    public override bool SharedOnDamage(
        WeaponFinalCache weaponCache,
        IStaticWorldObject targetObject,
        double damagePreMultiplier,
        out double obstacleBlockDamageCoef,
        out double damageApplied)
    {
      if (weaponCache.Character is not null)
      {
        // damaged by character
        if (IsServer)
        {
          // notify other characters
          using var tempList = Api.Shared.GetTempList<ICharacter>();
          Server.World.GetScopedByPlayers(targetObject, tempList);
          tempList.Remove(weaponCache.Character);
          this.CallClient(tempList.AsList(), _ => _.ClientRemote_OnHit());
        }
        else
        {
          ClientOnHit();
        }
      }

      if (weaponCache.ProtoWeapon is not null
          && !(weaponCache.ProtoWeapon is IProtoItemWeaponMelee))
      {
        // hit but not damaged - only melee weapons (including pickaxes can damage this)
        obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
        damageApplied = 0;
        return true;
      }

      return base.SharedOnDamage(weaponCache,
                                 targetObject,
                                 damagePreMultiplier,
                                 out obstacleBlockDamageCoef,
                                 out damageApplied);
    }

    /// <summary>
    /// When the explosion is processed this method will be called (see ObjectMineralPragmiumSourceExplosion).
    /// </summary>
    internal static void ServerOnExplode(
        Vector2Ushort epicenterPosition,
        double explosionRadius,
        ICharacter byCharacter)
    {
      // let's spawn scattered pragmium nodes around the explosion site 
      var countToSpawnRemains = DestroySpawnNodeCount;
      var attemptsRemains = 2000;

      while (countToSpawnRemains > 0)
      {
        attemptsRemains--;
        if (attemptsRemains <= 0)
        {
          // attempts exceeded
          return;
        }

        // calculate random distance from the explosion epicenter
        var distance = RandomHelper.Range(2, explosionRadius);

        // ensure we spawn more objects closer to the epicenter
        var spawnProbability = 1 - (distance / explosionRadius);
        spawnProbability = Math.Pow(spawnProbability, 1.5);
        if (!RandomHelper.RollWithProbability(spawnProbability))
        {
          // random skip
          continue;
        }

        var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
        var spawnPosition = new Vector2Ushort(
            (ushort)(epicenterPosition.X + distance * Math.Cos(angle)),
            (ushort)(epicenterPosition.Y + distance * Math.Sin(angle)));

        // try spawn a pragmium node
        if (ServerTrySpawnNode(spawnPosition, pveTagForCharacter: byCharacter))
        {
          // spawned successfully!
          countToSpawnRemains--;
        }

      }
    }

    protected override ITextureResource ClientGetTextureResource(IStaticWorldObject gameObject, PublicState state)
    {
      return this.DefaultTexture;
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);
      ObjectMineralPragmiumHelper.ClientInitializeLightForSource(data.GameObject.ClientSceneObject);
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      renderer.SpritePivotPoint = (0, 0);
      renderer.PositionOffset = (0, 0);
      renderer.DrawOrderOffsetY = 1.0;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("####",
                   "####");

    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return new TextureResource(GenerateTexturePath(thisType));
    }

    protected override void PrepareProtoMineral(MineralDropItemsConfig config)
    {
      // drops nothing as there is a separate method for explosion
    }

    protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
    {
      base.PrepareTileRequirements(tileRequirements);
    }

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      if (!data.IsFirstTimeInit)
      {
        return;
      }

      var worldObject = data.GameObject;
      ServerTimersSystem.AddAction(
          1,
          () =>
          {
            if (worldObject.IsDestroyed)
            {
              return;
            }

            ServerTrySpawnNodesAround(worldObject);
            ServerTrySpawnMobs(worldObject);
          });
    }

    protected override void ServerOnStaticObjectZeroStructurePoints(
        WeaponFinalCache weaponCache,
        ICharacter byCharacter,
        IWorldObject targetObject)
    {
      var tilePosition = targetObject.TilePosition;
      var privateState = GetPrivateState((IStaticWorldObject)targetObject);

      base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);

      var objectExplosion = Server.World.CreateStaticWorldObject
          <ObjectMineralGiantPragmiumSourceExplosion>(tilePosition);

      ObjectMineralGiantPragmiumSourceExplosion.GetPublicState(objectExplosion).ExplodedByCharacter = byCharacter;
    }


    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          .AddShapeCircle(
              radius: 1.4,
              center: (2, 1.2))
          .AddShapeRectangle(
              size: (2, 1.5),
              offset: (1, 0.8),
              group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(
              size: (2, 1.5),
              offset: (1, 0.8),
              group: CollisionGroups.HitboxRanged)
          .AddShapeRectangle(
              size: (1.25, 4.1),
              offset: (1.25, 1),
              group: CollisionGroups.HitboxRanged);
    }

    private static void ClientOnHit()
    {
      const float shakesDuration = 0.1f,
                  shakesDistanceMin = 0.1f,
                  shakesDistanceMax = 0.125f;
      ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                        worldDistanceMin: -shakesDistanceMin,
                                                        worldDistanceMax: shakesDistanceMax);
    }

    private static void ServerTrySpawnMobs(IStaticWorldObject worldObject)
    {

      // calculate how many creatures are still alive
      var mobsList = GetPrivateState(worldObject).MobsList;

      var mobsAlive = 0;
      for (var index = 0; index < mobsList.Count; index++)
      {
        var character = mobsList[index];
        if (character.IsDestroyed)
        {
          mobsList.RemoveAt(index--);
          continue;
        }

        if (character.TilePosition.TileSqrDistanceTo(worldObject.TilePosition)
            > MobDespawnDistance * MobDespawnDistance)
        {
          // the guardian mob is too far - probably lured away by a player
          using var tempListObservers = Api.Shared.GetTempList<ICharacter>();
          Server.World.GetScopedByPlayers(character, tempListObservers);
          if (tempListObservers.Count == 0)
          {
            // despawn this mob as it's not observed by any player
            Server.World.DestroyObject(character);
            mobsList.RemoveAt(index--);
          }

          continue;
        }

        mobsAlive++;
      }

      var countToSpawn = MobsCountLimit - mobsAlive;
      if (countToSpawn <= 0)
      {
        return;
      }

      // spawn mobs(s) nearby
      countToSpawn = Math.Min(countToSpawn, ServerSpawnMobsMaxCountPerIteration);
      ServerMobSpawnHelper.ServerTrySpawnMobsCustom(protoMob: LazyProtoMob.Value,
                                                    spawnedCollection: mobsList,
                                                    countToSpawn,
                                                    excludeBounds: worldObject.Bounds.Inflate(1),
                                                    maxSpawnDistanceFromExcludeBounds: MobSpawnDistance,
                                                    noObstaclesCheckRadius: 0.5,
                                                    maxAttempts: 200);
    }

    private static bool ServerTrySpawnNode(Vector2Ushort spawnPosition, ICharacter pveTagForCharacter)
    {
      var protoNode = ProtoNodeLazy.Value;

      if (!protoNode.CheckTileRequirements(spawnPosition,
                                           character: null,
                                           out string errorMessage,
                                           logErrors: false))
      {
        if(!errorMessage.Contains("restricted area"))
          return false;
      }

      var node = Server.World.CreateStaticWorldObject(protoNode, spawnPosition);
      if (node is null)
      {
        return false;
      }


      return true;
    }

    private static void ServerTrySpawnNodesAround(IStaticWorldObject worldObject)
    {
      // calculate how many nodes are nearby
      var nodesAroundCount = 0;
      var neighborTiles = worldObject.OccupiedTiles
                                     .SelectMany(t => t.EightNeighborTiles)
                                     .Distinct()
                                     .ToList();

      foreach (var neighborTile in neighborTiles)
      {
        foreach (var otherObject in neighborTile.StaticObjects)
        {
          if (!(otherObject.ProtoStaticWorldObject is ObjectMineralPragmiumNode))
          {
            continue;
          }

          nodesAroundCount++;
          if (nodesAroundCount >= NodesCountLimit)
          {
            // there are already enough nodes around
            return;
          }
        }
      }

      // spawn node(s) nearby
      var countToSpawn = NodesCountLimit - nodesAroundCount;
      if (countToSpawn <= 0)
      {
        return;
      }

      var tag = GetPublicState(worldObject).WorldObjectClaim;
      var pveTagForCharacter = WorldObjectClaimSystem.ServerGetCharacterByClaim(tag);

      var attempts = neighborTiles.Count * 4;
      countToSpawn = Math.Min(countToSpawn, ServerSpawnNodesMaxCountPerIteration);

      while (attempts-- > 0)
      {
        var neighborTile = neighborTiles.TakeByRandom();
        if (neighborTile.StaticObjects.Count > 0)
        {
          // cannot spawn there
          continue;
        }

        if (!ServerTrySpawnNode(neighborTile.Position, pveTagForCharacter))
        {
          // cannot spawn there
          continue;
        }

        countToSpawn--;
        if (countToSpawn == 0)
        {
          return;
        }
      }
    }

    /// <summary>
    /// When the pragmium source is hit by a player it should dangerously shake! :-)
    /// </summary>
    [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
    private void ClientRemote_OnHit()
    {
      ClientOnHit();
    }

    public class PrivateState : BasePrivateState
    {
      [TempOnly]
      public List<ICharacter> MobsList { get; } = new();
    }

    public class PublicState : StaticObjectPublicState
    {
      [TempOnly]
      public double LastTimeObservedByAnyPlayer { get; set; }
    }
  }
}