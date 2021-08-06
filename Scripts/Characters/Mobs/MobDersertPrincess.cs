namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.BossLootSystem;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class MobDesertPrincess : ProtoCharacterMob
              <MobDesertPrincess.PrivateState,
              CharacterMobPublicState,
              CharacterMobClientState>,
              IProtoCharacterSmallBoss
  {
    private const int DeathSpawnLootObjectsDefaultCount = 2;

    private const double DeathSpawnLootObjectsRadius = 5;

    private const double MinionSpawnNoObstaclesCircleRadius = 0.25;

    private const double SpawnMinionsCheckDistance = 21;

    private const int SpawnMinionsPerPlayer = 1;

    private const int SpawnMinionsTotalNumberMax = 6;

    private const int SpawnMinionsTotalNumberMin = 1;

    public override double MobKillExperienceMultiplier => 1;

    // Determines how often the boss will attempt to use the nova attack (a time interval in seconds).
    private static readonly Interval<double> NovaAttackInterval
        = new(min: 5, max: 10);

    private static readonly Lazy<IProtoStaticWorldObject> ProtoLootObjectLazy
        = new(GetProtoEntity<ObjectDesertPrincessRemains>);

    private static readonly Lazy<IProtoStaticWorldObject> ProtoLootObjectLazy2
        = new(GetProtoEntity<ObjectMineralPragmiumSource>);

    private IReadOnlyList<AiWeaponPreset> weaponsListNovaAttack;

    private IReadOnlyList<AiWeaponPreset> weaponsListPrimary;

    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override float CharacterWorldHeight => 2.5f;

    public override bool HasIncreasedScopeSize => true;

    public override string Name => "Desert Princess";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

    public override double StatDefaultHealthMax => 1500;

    public override double StatMoveSpeed => 2.25;

    protected override byte SoundEventsNetworkRadius => 20;

    public override void ServerOnDeath(ICharacter character)
    {
      this.ServerSendDeathSoundEvent(character);

      ServerTimersSystem.AddAction(
          delaySeconds: 3,
          () =>
          {
            var bossPosition = character.Position;

            // explode
            var protoExplosion = Api.GetProtoEntity<ObjectDesertPrincessDeathExplosion>();
            Server.World.CreateStaticWorldObject(protoExplosion,
                                                       (bossPosition - protoExplosion.Layout.Center)
                                                       .ToVector2Ushort());

            var privateState = GetPrivateState(character);
            var damageTracker = privateState.DamageTracker;

            // spawn loot and minions on death
            ServerTimersSystem.AddAction(
                delaySeconds: protoExplosion.ExplosionDelay.TotalSeconds
                              + protoExplosion.ExplosionPreset.ServerDamageApplyDelay * 1.01,
                () =>
            {
              ServerBossLootSystem.ServerCreateBossLoot(
                        bossPosition: bossPosition.ToVector2Ushort(),
                        protoCharacterBoss: this,
                        character: character,
                        damageTracker: damageTracker,
                        bossDifficultyCoef: 1.0,
                        lootObjectProto: ProtoLootObjectLazy.Value,
                        //lootObjectProto: RandomHelper.Next(2) == 1 ? ProtoLootObjectLazy.Value : ProtoLootObjectLazy2.Value,
                        lootObjectsDefaultCount: DeathSpawnLootObjectsDefaultCount,
                        lootObjectsRadius: DeathSpawnLootObjectsRadius,
                        learningPointsBonusPerLootObject: 5,
                        maxLootWinners: 3);
            });

            // destroy the character object after the explosion
            ServerTimersSystem.AddAction(
                delaySeconds: protoExplosion.ExplosionDelay.TotalSeconds + 0.5,
                () => Server.World.DestroyObject(character));
          });
    }

    public void ServerTrySpawnMinions(ICharacter characterBoss)
    {
      var bossDamageTracker = GetPrivateState(characterBoss).DamageTracker;
      var bossPosition = characterBoss.Position + (0, 1.0);

      // calculate how many minions required
      var minionsRequired = 0;
      using var tempListCharacters = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(characterBoss, tempListCharacters);

      foreach (var player in tempListCharacters.AsList())
      {
        if (player.Position.DistanceSquaredTo(bossPosition)
            <= SpawnMinionsCheckDistance * SpawnMinionsCheckDistance)
        {
          minionsRequired += SpawnMinionsPerPlayer;
        }
      }

      minionsRequired = MathHelper.Clamp(minionsRequired,
                                         SpawnMinionsTotalNumberMin,
                                         SpawnMinionsTotalNumberMax);

      // spawn minions
      var attemptsRemains = 300;
      var physicsSpace = characterBoss.PhysicsBody.PhysicsSpace;

      const double spawnDistanceMin = 2.0;
      const double spawnDistanceMax = 3.0;
      while (minionsRequired > 0)
      {
        attemptsRemains--;
        if (attemptsRemains <= 0)
        {
          // attempts exceeded
          return;
        }

        var spawnDistance = spawnDistanceMin
                            + RandomHelper.NextDouble() * (spawnDistanceMax - spawnDistanceMin);
        var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;

        var publicState = GetPublicState(characterBoss);
        var deg = publicState.AppliedInput.RotationAngleDeg;
        var distanceX = spawnDistance * Math.Cos(angle);
        var distanceY = spawnDistance * Math.Sin(angle);
        _ = ((deg >= 0 && deg <= 90) || (deg >= 270 && deg <= 360)) ? distanceX = Math.Abs(distanceX) : distanceX = -Math.Abs(distanceX);
        _ = (deg >= 0 && deg <= 180) ? distanceY = Math.Abs(distanceY) : distanceY = -Math.Abs(distanceY);

        var spawnPosition = new Vector2Ushort(
            (ushort)(bossPosition.X + distanceX),
            (ushort)(bossPosition.Y + distanceY));

        if (ServerTrySpawnMinion(spawnPosition))
        {
          // spawned successfully!
          minionsRequired--;
        }
      }

      bool ServerTrySpawnMinion(Vector2Ushort spawnPosition)
      {
        var worldPosition = spawnPosition.ToVector2D();
        if (physicsSpace.TestCircle(worldPosition,
                                    MinionSpawnNoObstaclesCircleRadius,
                                    CollisionGroups.Default,
                                    sendDebugEvent: true).EnumerateAndDispose().Any())
        {
          // obstacles nearby
          return false;
        }

        if (mobs is null)
        {
          mobs = Api.FindProtoEntities<ProtoCharacterMob>();
          mobs.RemoveAll(m => m.StatDefaultHealthMax < 80);
          mobs.RemoveAll(m => m.GetType().ToString().Contains("NPC"));
        }

        int r = RandomHelper.Next(mobs.Count);

        ProtoCharacterMob mob = mobs[r];

        var spawnedCharacter = Server.Characters.SpawnCharacter(mob, worldPosition);

        if (spawnedCharacter is null)
        {
          return false;
        }

        return true;
      }
    }

    static List<ProtoCharacterMob> mobs = null;

    public override bool SharedOnDamage(
        WeaponFinalCache weaponCache,
        IWorldObject targetObject,
        double damagePreMultiplier,
        double damagePostMultiplier,
        out double obstacleBlockDamageCoef,
        out double damageApplied)
    {
      var byCharacter = weaponCache.Character;
      if (NewbieProtectionSystem.SharedIsNewbie(byCharacter))
      {
        // don't allow attacking a boss while under newbie protection
        if (IsClient)
        {
          NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(this);
        }

        obstacleBlockDamageCoef = 0;
        damageApplied = 0;
        return false;
      }

      if (IsServer)
      {
        if (weaponCache.ProtoExplosive is not null)
        {
          // the boss is massive so it will take increased damage from explosives/grenades
          if (weaponCache.ProtoExplosive is IAmmoCannonShell)
          {
            damagePostMultiplier *= 1.333; // the artillery shells are too OP already
          }
          else
          {
            damagePostMultiplier *= 2;
          }
        }
      }

      var result = base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       damagePostMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);

      if (IsServer
          && result
          && byCharacter is not null
          && !byCharacter.IsNpc)
      {
        var privateState = GetPrivateState((ICharacter)targetObject);

        // record the damage dealt by player
        privateState.DamageTracker.RegisterDamage(byCharacter, (ICharacter)targetObject, damageApplied);
      }

      return result;
    }

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 0.6)
             .AddValue(this, StatName.DefenseKinetic, 0.95)
             .AddValue(this, StatName.DefenseExplosion, 0.5)
             .AddValue(this, StatName.DefenseHeat, 0.75)
             .AddValue(this, StatName.DefenseCold, 0.7)
             .AddValue(this, StatName.DefenseChemical, 1.0)
             .AddValue(this, StatName.DefensePsi, 1.0)
             .AddPercent(this, StatName.DazedIncreaseRateMultiplier, -100);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonDesertPrincess>();
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      data.PrivateState.DamageTracker = new ServerBossDamageTracker();

      this.weaponsListPrimary = new AiWeaponPresetList()
                                .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponMobDesertPrincessMelee>()))
                                .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponMobDesertPrincessRanged>()))
                                .ToReadReadOnly();

      this.weaponsListNovaAttack = new AiWeaponPresetList()
                                   .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponMobDesertPrincessNova>()))
                                   .ToReadReadOnly();

      ServerMobWeaponHelper.TrySetWeapon(data.GameObject,
                                         this.weaponsListPrimary[0].ProtoWeapon,
                                         rebuildWeaponsCacheNow: false);
    }

    protected override void ServerUpdateMob(ServerUpdateData data)
    {
      var character = data.GameObject;
      var publicState = data.PublicState;

      if (publicState.IsDead)
      {
        return;
      }

      var privateState = data.PrivateState;
      var lastTargetCharacter = privateState.CurrentTargetCharacter;
      var deltaTime = data.DeltaTime;

      data.PrivateState.DamageTracker.Update(deltaTime);

      publicState.CurrentStats.ServerSetHealthCurrent(
          (float)(publicState.CurrentStats.HealthCurrent));

      var weaponList = this.ServerSelectWeaponsList(privateState,
                                                    deltaTime,
                                                    out var isSwitchingToNovaAttack);

      ServerCharacterAiHelper.ProcessBossAi(
          character,
          weaponList,
          distanceEnemyTooClose: 7.5,
          distanceEnemyTooFar: 15.5,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      if (lastTargetCharacter is null
          && privateState.CurrentTargetCharacter is not null
          // is the last attack happened not too recently?
          && privateState.TimeToNextNovaAttack < NovaAttackInterval.Max - 8)
      {
        //Logger.Dev("Boss acquired target! Will use a nova attack in the next 2-4 seconds!");
        privateState.TimeToNextNovaAttack = RandomHelper.Next(2, 4);
      }

      if (isSwitchingToNovaAttack)
      {
        movementDirection = default;
        privateState.WeaponState.SharedSetInputIsFiring(false);
      }
      else if (privateState.WeaponState.IsFiring
               && privateState.WeaponState.ProtoWeapon is ItemWeaponMobDesertPrincessNova)
      {
        movementDirection = default;
      }

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }

    private IReadOnlyList<AiWeaponPreset> ServerSelectWeaponsList(
        PrivateState privateState,
        double deltaTime,
        out bool isSwitchingToNovaAttack)
    {
      var weaponList = this.weaponsListPrimary;
      isSwitchingToNovaAttack = false;

      privateState.TimeToNextNovaAttack -= deltaTime;
      if (privateState.TimeToNextNovaAttack > 0)
      {
        return weaponList;
      }

      if (privateState.CurrentTargetCharacter is null
          || privateState.WeaponState.CooldownSecondsRemains > 0
          || privateState.WeaponState.DamageApplyDelaySecondsRemains > 0)
      {
        // cannot switch to nova attack right now as the previous attack is ongoing
        // stop the previous attack and wait until it's finished
        isSwitchingToNovaAttack = true;
        privateState.TimeToNextNovaAttack = 0;
      }
      else
      {
        // time to start a nova attack
        privateState.TimeToNextNovaAttack = NovaAttackInterval.Min;
        privateState.TimeToNextNovaAttack += RandomHelper.NextDouble()
                                             * (NovaAttackInterval.Max - NovaAttackInterval.Min);

        weaponList = this.weaponsListNovaAttack;
        //Logger.Dev("Time to start a nova attack!");
      }

      return weaponList;
    }

    public class PrivateState : CharacterMobPrivateState
    {
      [TempOnly]
      public ServerBossDamageTracker DamageTracker { get; set; }

      public double TimeToNextNovaAttack { get; set; }
    }
  }
}