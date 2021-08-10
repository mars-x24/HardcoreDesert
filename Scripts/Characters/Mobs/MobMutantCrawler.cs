namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.GameApi.Data.World;

  public class MobMutantCrawler : ProtoCharacterMob
  {
    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override float CharacterWorldHeight => 1.1f;

    public override float CharacterWorldWeaponOffsetRanged => 0.2f;

    public override double MobKillExperienceMultiplier => 0.7;

    public override string Name => "Mutant crawler";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

    public override double StatDefaultHealthMax => 140;

    public override double StatMoveSpeed => 2.3;

    public override double StatMoveSpeedRunMultiplier => 1.3;

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 0.4)
             .AddValue(this, StatName.DefenseKinetic, 0.9)
             .AddValue(this, StatName.DefenseExplosion, 0.1)
             .AddValue(this, StatName.DefenseHeat, 0.1)
             .AddValue(this, StatName.DefenseChemical, 0.8)
             .AddValue(this, StatName.DefenseCold, 0.9)
             .AddValue(this, StatName.DefensePsi, 1.0)
             .AddValue(this, StatName.DefenseRadiation, 1.0);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonMutantCrawler>();

      // primary loot
      lootDroplist
          .Add<ItemToxin>(count: 1)
          .Add(nestedList: new DropItemsList(outputs: 1)
                           .Add<ItemInsectMeatRaw>(count: 1)
                           .Add<ItemBones>(count: 1)
                           .Add<ItemSlime>(count: 1)
          // requires device
          .Add<ItemKeiniteRaw>(count: 1, countRandom: 1, condition: ItemKeiniteCollector.ConditionHasDeviceEquipped));

      // extra loot
      lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                       nestedList: new DropItemsList(outputs: 1)
                                   .Add<ItemInsectMeatRaw>(count: 1)
                                   .Add<ItemToxin>(count: 1)
                                   .Add<ItemBones>(count: 1)
                                   .Add<ItemSlime>(count: 2));
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobMutantCrawlerPoison>();
      data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
      data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);

      // schedule destruction by timer
      var gameObject = data.GameObject;
      ServerTimersSystem.AddAction(
          delaySeconds: 60 * 60, // 60 minutes
          () => ServerDespawnTimerCallback(gameObject));
    }

    private static void ServerDespawnTimerCallback(IWorldObject gameObject)
    {
      if (!Server.World.IsObservedByAnyPlayer(gameObject))
      {
        // can destroy now
        Server.World.DestroyObject(gameObject);
        return;
      }

      // postpone destruction
      ServerTimersSystem.AddAction(
          delaySeconds: 60,
          () => ServerDespawnTimerCallback(gameObject));
    }

    protected override void ServerUpdateMob(ServerUpdateData data)
    {
      var character = data.GameObject;
      var currentStats = data.PublicState.CurrentStats;

      ServerCharacterAiHelper.ProcessAggressiveAi(
          character,
          targetCharacter: ServerCharacterAiHelper.GetClosestTargetPlayer(character),
          isRetreating: false,
          isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
          distanceRetreat: 0,
          distanceEnemyTooClose: 2,
          distanceEnemyTooFar: 8,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }
  }
}