namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;

  public class MobEnragedWildBoar : ProtoCharacterMobEnraged
  {
    public override float CharacterWorldHeight => 1f;

    public override double MobKillExperienceMultiplier => 1.0;

    public override string Name => "Enraged Wild boar";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

    public override double StatDefaultHealthMax => 100;

    public override double StatMoveSpeed => 2.1;

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
      skeleton = GetProtoEntity<SkeletonWildBoar>();

      // primary loot
      lootDroplist
          .Add<ItemMeatRaw>(count: 1)
          .Add<ItemLeather>(count: 1, probability: 1 / 2.0);

      // random loot
      lootDroplist.Add(nestedList: new DropItemsList(outputs: 1)
                                   .Add<ItemAnimalFat>(count: 1)
                                   .Add<ItemBones>(count: 1));
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
          targetStructure: ServerEnragedAiHelper.GetClosestTargetStructure(character),
          targetCharacter: ServerEnragedAiHelper.GetClosestTargetPlayer(character),
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