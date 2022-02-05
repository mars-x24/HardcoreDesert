﻿namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Events;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Alien;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.BossLootSystem;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.CoreMod.Systems.WorldDiscovery;
  using AtomicTorch.CBND.CoreMod.Zones;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using static AtomicTorch.CBND.CoreMod.Systems.BossLootSystem.ServerBossLootSystem;

  public class MobBossPragmiumKing
      : ProtoCharacterMob
        <MobBossPragmiumKing.PrivateState,
            CharacterMobPublicState,
            CharacterMobClientState>,
        IProtoCharacterBoss
  {
    // The boss is regenerating fast only if it didn't receive a significant damage for a while.
    private const double BossHealthRegenerationPerSecondFast = 40;

    private const double BossHealthRegenerationPerSecondSlow = 0;

    private const int DeathSpawnLootObjectsDefaultCount = 16;

    private const double DeathSpawnLootObjectsRadius = 19;

    private const int DeathSpawnMinionsDefaultCount = 8;

    private const double DeathSpawnMinionsRadius = 17;

    private const double MinionSpawnNoObstaclesCircleRadius = 2.0;

    // The boss can move within the area in configured radius only.
    private const double MovementMaxRadius = 20;

    // Delay since the last damage before the HP regeneration will start.
    private const double RegenerationDelaySeconds = 360;

    private const double SpawnMinionsCheckDistance = 21;

    private const int SpawnMinionsPerPlayer = 1;

    private const int SpawnMinionsTotalNumberMax = 20;

    private const int SpawnMinionsTotalNumberMin = 1;

    private const double VictoryLearningPointsBonusPerLootObject = 15;

    // Determines how often the boss will attempt to use the nova attack (a time interval in seconds).
    private static readonly Interval<double> NovaAttackInterval
        = new(min: 10, max: 14);

    private const int NovaAttackPerEnergyShieldAttack = 2;

    private static readonly Lazy<IProtoStaticWorldObject> ProtoLootObjectLazy
        = new(GetProtoEntity<ObjectPragmiumKingRemains>);


    // Determines the type of the minion creatures to spawn when boss is dead.
    private static readonly Lazy<IProtoCharacterMob> ProtoMinionObjectDeathLazy
        = new(GetProtoEntity<MobPragmiumBeetleMinion>);

    // Determines the type of the minion creatures to spawn when boss is using the nova attack.
    private static readonly Lazy<IProtoCharacterMob> ProtoMinionObjectLazy
        = new(GetProtoEntity<MobPragmiumBeetleMinion>);

    private static readonly double ServerBossDifficultyCoef;

    private static readonly int ServerMaxLootWinners;

    private IReadOnlyList<AiWeaponPreset> weaponsListNovaAttack;

    private IReadOnlyList<AiWeaponPreset> weaponsListEnergyShield;

    private IReadOnlyList<AiWeaponPreset> weaponsListPrimary;


    static MobBossPragmiumKing()
    {
      if (Api.IsClient)
      {
        return;
      }

      var requiredPlayersNumber = RateBossDifficultyPragmiumKing.SharedValue;

      // coef range from 0.2 to 2.0
      ServerBossDifficultyCoef = requiredPlayersNumber / 5.0;

      ServerMaxLootWinners = (int)Math.Ceiling(requiredPlayersNumber * 2);
      ServerMaxLootWinners = Math.Max(ServerMaxLootWinners, 5); // ensure at least 5 winners
    }

    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override float CharacterWorldHeight => 3.15f;

    public override bool HasIncreasedScopeSize => true;

    // it's a boss and currently we don't have a way to account and add the kill to all participating players
    public override bool IsAvailableInCompletionist => false;

    public override bool IsBoss => true;

    // no experience for killing it, as it would just go to the player who dealt the final hit
    // instead, there is a different mechanic which provides XP to every participating player
    public override double MobKillExperienceMultiplier => 0;

    public override string Name => "Pragmium King";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

    public override double StatDefaultHealthMax => 9000;

    // Please note: boss using different regeneration mechanism.
    public override double StatHealthRegenerationPerSecond => 0;

    public override double StatMoveSpeed => 1.5;

    protected override byte SoundEventsNetworkRadius => 100;

    public override void ServerOnDeath(ICharacter character)
    {
      this.ServerSendDeathSoundEvent(character);

      ServerTimersSystem.AddAction(
          delaySeconds: 3,
          () =>
          {
            // explode
            var bossPosition = character.Position;
            var protoExplosion = Api.GetProtoEntity<ObjectPragmiumKingDeathExplosion>();
            Server.World.CreateStaticWorldObject(protoExplosion,
                                                         (bossPosition - protoExplosion.Layout.Center)
                                                         .ToVector2Ushort());

            var privateState = GetPrivateState(character);
            var damageTracker = privateState.DamageTracker;

            // spawn loot and minions on death
            ServerTimersSystem.AddAction(
                delaySeconds: protoExplosion.ExplosionDelay.TotalSeconds
                              + protoExplosion.ExplosionPreset.ServerDamageApplyDelay * 1.05,
                () =>
                {
                  List<WinnerEntry> winners = null;

                  try
                  {
                    winners = ServerBossLootSystem.ServerCreateBossLoot(
                                    bossPosition: bossPosition.ToVector2Ushort(),
                                    protoCharacterBoss: this,
                                    character: character, //MOD
                                    damageTracker: damageTracker,
                                    bossDifficultyCoef: ServerBossDifficultyCoef,
                                    lootObjectProto: ProtoLootObjectLazy.Value,
                                    lootObjectsDefaultCount: DeathSpawnLootObjectsDefaultCount,
                                    lootObjectsRadius: DeathSpawnLootObjectsRadius,
                                    learningPointsBonusPerLootObject: VictoryLearningPointsBonusPerLootObject,
                                    maxLootWinners: ServerMaxLootWinners);
                  }
                  finally
                  {
                    ServerBossLootSystem.ServerSpawnBossMinionsOnDeath(
                                    epicenterPosition: bossPosition.ToVector2Ushort(),
                                    bossDifficultyCoef: ServerBossDifficultyCoef,
                                    minionProto: ProtoMinionObjectDeathLazy.Value,
                                    minionsDefaultCount: DeathSpawnMinionsDefaultCount,
                                    minionsRadius: DeathSpawnMinionsRadius);

                    this.SpawnSalt(bossPosition.ToVector2Ushort(), 150.0);

                    this.SpawnTeleport(winners);
                  }
                });

            // destroy the character object after the explosion
            ServerTimersSystem.AddAction(
                delaySeconds: protoExplosion.ExplosionDelay.TotalSeconds + 0.5,
                () => Server.World.DestroyObject(character));
          });
    }

    private void SpawnSalt(Vector2Ushort epicenterPosition, double explosionRadius)
    {
      ZoneEventFinalBoss zone = Api.GetProtoEntity<ZoneEventFinalBoss>();
      if (zone is null || zone.ServerZoneInstance.IsEmpty)
        return;

      var countToSpawnRemains = 300;
      var attemptsRemains = 2000;

      var protoNode = Api.GetProtoEntity<ObjectMineralSalt>();

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


        if (zone.ServerZoneInstance.IsContainsPosition(spawnPosition))
        {
          // try spawn a pragmium node
          if (ServerTrySpawnNode(protoNode, spawnPosition))
          {
            // spawned successfully!
            countToSpawnRemains--;
          }
        }
      }
    }

    private void SpawnTeleport(List<WinnerEntry> winners)
    {
      var teleports = Server.World.GetStaticWorldObjectsOfProto<ObjectPropAlienTeleportPragmiumKing>();
      foreach (var teleport in teleports)
      {
        var tilePosition = teleport.TilePosition;
        Server.World.DestroyObject(teleport);
        var newTeleport = Server.World.CreateStaticWorldObject<ObjectAlienTeleport>(tilePosition);
        
        if (newTeleport is null)
          continue;

        if (winners is null)
          continue;

        foreach (var winner in winners)
        {
          WorldDiscoverySystem.Instance.ServerDiscoverWorldChunks(
            winner.Character,
            newTeleport.OccupiedTilePositions.ToList());
        }
      }
    }

    private static bool ServerTrySpawnNode(ProtoObjectMineral protoNode, Vector2Ushort spawnPosition)
    {

      if (!protoNode.CheckTileRequirements(spawnPosition,
                                           character: null,
                                           logErrors: false))
      {
        return false;
      }

      var node = Server.World.CreateStaticWorldObject(protoNode, spawnPosition);
      if (node is null)
      {
        return false;
      }

      return true;
    }

    public void ServerTrySpawnMinions(ICharacter characterBoss)
    {
      ServerTrySpawnMinions(characterBoss, 2.0, 4.0);
    }

    public void ServerTrySpawnMinions(ICharacter characterBoss, double spawnDistanceMin, double spawnDistanceMax, IProtoCharacterMob mob = null)
    {
      if (mob == null)
        mob = ProtoMinionObjectLazy.Value;

      var bossDamageTracker = GetPrivateState(characterBoss).DamageTracker;
      var bossPosition = characterBoss.Position + (0, 1.0);

      // calculate how many minions required
      var minionsRequired = 1;
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

      // calculate how many minions present
      tempListCharacters.Clear();
      Server.World.GetScopedByCharacters(characterBoss, tempListCharacters.AsList(), onlyPlayerCharacters: false);

      var minionsHave = 0;
      var protoMobMinion = mob;
      foreach (var otherCharacter in tempListCharacters.AsList())
      {
        if (otherCharacter.IsNpc
            && otherCharacter.ProtoGameObject == protoMobMinion
            && otherCharacter.Position.DistanceSquaredTo(bossPosition)
            <= SpawnMinionsCheckDistance * SpawnMinionsCheckDistance
            && !otherCharacter.GetPublicState<CharacterMobPublicState>().IsDead)
        {
          minionsHave++;
        }
      }

      //Logger.Dev($"Minions required: {minionsRequired} minions have: {minionsHave}");
      minionsRequired -= minionsHave;
      if (minionsRequired <= 0)
      {
        return;
      }

      // spawn minions
      var attemptsRemains = 300;
      var physicsSpace = characterBoss.PhysicsBody.PhysicsSpace;

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
        var spawnPosition = new Vector2Ushort(
            (ushort)(bossPosition.X + spawnDistance * Math.Cos(angle)),
            (ushort)(bossPosition.Y + spawnDistance * Math.Sin(angle)));

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

        var spawnedCharacter = Server.Characters.SpawnCharacter(protoMobMinion, worldPosition);
        if (spawnedCharacter is null)
        {
          return false;
        }

        // write this boss' damage tracker into the minion character
        // so any damage dealt to it will be counted in the winners ranking
        var privateState = spawnedCharacter.GetPrivateState<ICharacterPrivateStateWithBossDamageTracker>();
        if (privateState is not null)
          privateState.DamageTracker = bossDamageTracker;
        return true;
      }
    }

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
        // apply the difficulty coefficient
        damagePostMultiplier /= ServerBossDifficultyCoef;

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
        // record the damage dealt by player
        var targetCharacter = (ICharacter)targetObject;
        var privateState = GetPrivateState(targetCharacter);
        // calculate the original damage (without the applied difficulty coefficient)
        var originalDamageApplied = damageApplied * ServerBossDifficultyCoef;
        privateState.DamageTracker.RegisterDamage(byCharacter,
                                                  targetCharacter,
                                                  originalDamageApplied);

        if (originalDamageApplied > 3)
        {
          // record the last time a significant damage is dealt
          privateState.LastDamageTime = Server.Game.FrameTime;
        }
      }

      return result;
    }

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 0.6)
             .AddValue(this, StatName.DefenseKinetic, 0.95)
             .AddValue(this, StatName.DefenseExplosion, 0.5)
             .AddValue(this, StatName.DefenseHeat, 0.7)
             .AddValue(this, StatName.DefenseCold, 0.7)
             .AddValue(this, StatName.DefenseChemical, 1.0)
             .AddValue(this, StatName.DefensePsi, 1.0)
             .AddPercent(this, StatName.DazedIncreaseRateMultiplier, -100);

      effects.AddValue(this, StatName.HealthMax, this.StatDefaultHealthMax);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonPragmiumKing>();

      if (!IsServer)
      {
        return;
      }

      ServerBossLootSystem.BossDefeated += ServerBossDefeatedHandler;

      static void ServerBossDefeatedHandler(
          IProtoCharacterMob protoCharacterBoss,
          Vector2Ushort bossPosition,
          List<ServerBossLootSystem.WinnerEntry> winnerEntries)
      {
        if (protoCharacterBoss.GetType() != typeof(MobBossPragmiumKing))
        {
          return;
        }

        foreach (var entry in winnerEntries)
        {
          PlayerCharacter.GetPrivateState(entry.Character)
                         .CompletionistData
                         .ServerOnParticipatedInEvent(Api.GetProtoEntity<EventBossPragmiumKing>());
        }
      }
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      if (data.IsFirstTimeInit)
      {
        data.PrivateState.HoldPosition = data.GameObject.TilePosition;
      }

      data.PrivateState.DamageTracker = new ServerBossDamageTracker();

      var kingMelee = GetProtoEntity<ItemWeaponMobPragmiumKingMelee>();
      var kingRanged = GetProtoEntity<ItemWeaponMobPragmiumKingRanged>();
      var kingShield = GetProtoEntity<ItemWeaponMobPragmiumKingMinion>();

      this.weaponsListPrimary = new AiWeaponPresetList()
                                .Add(new AiWeaponPreset(kingMelee))
                                .Add(new AiWeaponPreset(kingRanged))
                                .ToReadReadOnly();

      var kingNova = GetProtoEntity<ItemWeaponMobPragmiumKingNova>();

      this.weaponsListNovaAttack = new AiWeaponPresetList()
                                   .Add(new AiWeaponPreset(kingNova))
                                   .ToReadReadOnly();

      this.weaponsListEnergyShield = new AiWeaponPresetList()
                             .Add(new AiWeaponPreset(kingShield))
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

      // Regenerate the health points fast on every frame
      // if there was no damage dealt to boss recently.
      // Please note: the difficulty coefficient doesn't apply there
      // as the boss HP doesn't change with difficulty - only damage
      // to it is modified by the difficulty coefficient.
      var isRegeneratingFast = Server.Game.FrameTime
                               >= privateState.LastDamageTime + RegenerationDelaySeconds;

      var regenerationPerSecond
          = isRegeneratingFast
                ? BossHealthRegenerationPerSecondFast
                : BossHealthRegenerationPerSecondSlow;

      publicState.CurrentStats.ServerSetHealthCurrent(
          (float)(publicState.CurrentStats.HealthCurrent
                  + regenerationPerSecond * deltaTime));

      var weaponList = this.ServerSelectWeaponsList(privateState,
                                                    deltaTime,
                                                    out var isSwitchingToNovaAttack);

      ServerCharacterAiHelper.ProcessBossAi(
          character,
          weaponList,
          distanceEnemyTooClose: 6,
          distanceEnemyTooFar: 25,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      if (movementDirection != default
          && !ServerCanMoveInDirection(character.TilePosition.ToVector2D(),
                                       movementDirection,
                                       privateState.HoldPosition.ToVector2D()))
      {
        // cannot move in desired direction - too far from the position to hold
        movementDirection = default;
      }

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
               && privateState.WeaponState.ProtoWeapon is ItemWeaponMobPragmiumKingNova)
      {
        movementDirection = default;
      }

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }

    private static bool ServerCanMoveInDirection(
        in Vector2D currentPosition,
        in Vector2F movementDirection,
        in Vector2D holdPosition)
    {
      var deltaPos = currentPosition + movementDirection.Normalized - holdPosition;
      return deltaPos.LengthSquared <= MovementMaxRadius * MovementMaxRadius;
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

        if (privateState.CountNovaAttack > 1 && privateState.CountNovaAttack % NovaAttackPerEnergyShieldAttack == 0)
          weaponList = this.weaponsListEnergyShield;
        else
          weaponList = this.weaponsListNovaAttack;

        privateState.CountNovaAttack++;

        //Logger.Dev("Time to start a nova attack!");
      }

      return weaponList;
    }

    public class PrivateState : CharacterMobPrivateState
    {
      [TempOnly]
      public ServerBossDamageTracker DamageTracker { get; set; }

      public Vector2Ushort HoldPosition { get; set; }

      [TempOnly]
      public double LastDamageTime { get; set; }

      public double TimeToNextNovaAttack { get; set; }

      public int CountNovaAttack { get; set; }
    }
  }
}