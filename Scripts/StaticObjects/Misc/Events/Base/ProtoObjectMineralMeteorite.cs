namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.CoreMod.Items.Tools;
  using AtomicTorch.CBND.CoreMod.Items.Weapons;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
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
          <ObjectMineralMeteoritePrivateState,
              ObjectMineralMeteoritePublicState,
              DefaultMineralClientState>,
          IProtoObjectEventEntry
  {

    //MOD
    // mob spawn parameters begins here //
    private static readonly Lazy<IProtoCharacter> LazyProtoMob = new(GetProtoEntity<MobMutantCrawler>);

    private const int MobDespawnDistance = 20;

    // How many guardian mobs each meteorite can have simultaneously.
    private const int MobsCountLimit = 3;

    private const int MobSpawnDistance = 8;

    // How many guardian mobs a meteorite will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
    private const int ServerSpawnMobsMaxCountPerIteration = 3; // spawn at max 6 mobs per iteration

    // end //

    public override bool IsAllowDroneMining => false;

    public override bool IsAllowQuickMining => true;

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

      //MOD
      // .Add(ConstructionTileRequirements.ValidatorNotRestrictedAreaEvenForServer)
      // .Add(ConstructionTileRequirements.ValidatorTileNotRestrictingConstructionEvenForServer);
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


            ServerMobSpawnHelper.ServerTrySpawnMobs(worldObject, GetPrivateState(worldObject).MobsList,
              MobSpawnDistance, MobDespawnDistance, MobsCountLimit, ServerSpawnMobsMaxCountPerIteration, LazyProtoMob?.Value);
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

  }
}