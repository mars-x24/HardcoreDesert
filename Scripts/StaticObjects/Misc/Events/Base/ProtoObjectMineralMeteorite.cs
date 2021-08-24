namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.CoreMod.Items.Tools;
  using AtomicTorch.CBND.CoreMod.Items.Weapons;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.CoreMod.UI;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Extensions;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System;

  public abstract class ProtoObjectMineralMeteorite
      : ProtoObjectMineral
        <ObjectMineralPragmiumSource.PrivateState,
            ObjectMineralMeteoritePublicState,
            DefaultMineralClientState>,
        IProtoObjectEventEntry
  {
    private readonly double serverRateLootCountMultiplier;

    protected ProtoObjectMineralMeteorite()
    {
      if (IsServer)
      {
        this.serverRateLootCountMultiplier = ServerRates.Get(
            "DropListItemsCountMultiplier." + this.ShortId,
            defaultValue: 1.0,
            @"This rate determines the item droplist multiplier for loot in " + this.Name + ".");
      }
    }


    // mob spawn parameters begins here //

    private static readonly Lazy<IProtoCharacter> LazyProtoMob
        = new(GetProtoEntity<MobMutantCrawler>);

    private const int MobDespawnDistance = 20;

    // How many guardian mobs each meteorite can have simultaneously.
    private const int MobsCountLimit = 3;

    private const int MobSpawnDistance = 8;

    // How many guardian mobs a meteorite will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
    private const int ServerSpawnMobsMaxCountPerIteration = 3; // spawn at max 6 mobs per iteration

    // end //

    public override bool IsAllowDroneMining => false;

    public override bool IsAllowQuickMining => false;

    public abstract double ServerCooldownDuration { get; }

    protected override bool CanFlipSprite => true;

    protected override void ClientObserving(ClientObjectData data, bool isObserving)
    {
      ClientMeteoriteTooltipHelper.Refresh(data.GameObject, isObserving);
    }

    protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
    {
      base.PrepareTileRequirements(tileRequirements);
      tileRequirements.Add(LandClaimSystem.ValidatorFreeLandEvenForServer);
                     // .Add(ConstructionTileRequirements.ValidatorNotRestrictedAreaEvenForServer)
                     // .Add(ConstructionTileRequirements.ValidatorTileNotRestrictingConstructionEvenForServer);
    }

    protected override double ServerGetDropListProbabilityMultiplier(IStaticWorldObject mineralObject)
    {
      // compensate for the general server items drop rate
      // but apply a separate rate
      return this.serverRateLootCountMultiplier
             / DropItemsList.DropListItemsCountMultiplier;
    }

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      if (data.IsFirstTimeInit)
      {
        data.PublicState.CooldownUntilServerTime
            = Server.Game.FrameTime + this.ServerCooldownDuration;
      }

      //
      var worldObject = data.GameObject;
      ServerTimersSystem.AddAction(
          1,
          () =>
          {
            if (worldObject.IsDestroyed)
            {
              return;
            }

            ServerTrySpawnMobs(worldObject);
          });
      //
    }

    protected override double SharedCalculateDamageByWeapon(
        WeaponFinalCache weaponCache,
        double damagePreMultiplier,
        IStaticWorldObject targetObject,
        out double obstacleBlockDamageCoef)
    {
      var serverTime = IsServer
                           ? Server.Game.FrameTime
                           : Client.CurrentGame.ServerFrameTimeApproximated;

      if (serverTime < GetPublicState(targetObject).CooldownUntilServerTime)
      {
        // too hot for mining - no damage to it
        if (IsClient
            && weaponCache.ProtoWeapon is IProtoItemToolMining)
        {
          NotificationSystem.ClientShowNotification(CoreStrings.Meteorite_CooldownMessage_TooHotForMining,
                                                    color: NotificationColor.Bad,
                                                    icon: this.Icon);
        }

        if (IsServer
            && weaponCache.ProtoWeapon is IProtoItemWeaponMelee
            && !weaponCache.Character.IsNpc)
        {
          weaponCache.Character.ServerAddStatusEffect<StatusEffectHeat>(intensity: 0.5);
        }

        obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
        return 0;
      }

      // meteorite cooldown finished
      if (NewbieProtectionSystem.SharedIsNewbie(weaponCache.Character))
      {
        // don't allow mining meteorite while under newbie protection
        if (IsClient)
        {
          NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(this);
        }

        obstacleBlockDamageCoef = 0;
        return 0;
      }

      return base.SharedCalculateDamageByWeapon(weaponCache,
                                                damagePreMultiplier,
                                                targetObject,
                                                out obstacleBlockDamageCoef);
    }

    //

    private static void ServerTrySpawnMobs(IStaticWorldObject worldObject)
    {
      if (LandClaimSystem.SharedIsLandClaimedByAnyone(worldObject.Bounds))
      {
        // don't spawn mobs as the land is claimed
        return;
      }

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
                                                    excludeBounds: worldObject.Bounds.Inflate(4),
                                                    maxSpawnDistanceFromExcludeBounds: MobSpawnDistance,
                                                    noObstaclesCheckRadius: 1.0,
                                                    maxAttempts: 200);
    }

    //
  }
}