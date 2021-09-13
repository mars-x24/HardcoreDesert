namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;

  public class MobEnragedWolf : ProtoCharacterMobEnraged
  {
    public override float CharacterWorldHeight => 1.1f;

    public override float CharacterWorldWeaponOffsetRanged => 0.4f;

    public override double MobKillExperienceMultiplier => 1.0;

    public override string Name => "Enraged Wolf";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

    public override double StatDefaultHealthMax => 100;

    public override double StatMoveSpeed => 1.9;

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 0.6);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonWolf>();

      // primary loot
      lootDroplist
          .Add<ItemFur>(count: 1, probability: 1 / 2.0)
          .Add<ItemBones>(count: 1)
          .Add<ItemAnimalFat>(count: 1, probability: 1 / 3.0);
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobEnragedGenericMedium>();
      data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
      data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
    }

    protected override void ServerUpdateMob(ServerUpdateData data)
    {
      var character = data.GameObject;

      ServerEnragedAiHelper.ProcessAggressiveAi(
          character,
          true,
          data.DeltaTime,
          distanceEnemyTooClose: 1,
          distanceAttackEnemyTooClose: 3,
          distanceEnemyTooFar: 25,
          secondsBeforeTryingGoalTarget: 60,
          secondsToAttackGoalTarget: 8,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }
  }
}