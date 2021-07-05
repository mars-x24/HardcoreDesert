namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Fishing;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;

  public class MobEnragedPragmiumBear : ProtoCharacterMobEnraged
  {
    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override float CharacterWorldHeight => 1.8f;

    public override float CharacterWorldWeaponOffsetRanged => 0.45f;

    public override double CorpseInteractionAreaScale => 1.25;

    public override double MobKillExperienceMultiplier => 2.0;

    public override string Name => "Pragmium Bear";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

    public override double StatDefaultHealthMax => 1000;

    public override double StatMoveSpeed => 1.8;

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 1.0);
      effects.AddValue(this, StatName.DefenseKinetic, 0.3);
      effects.AddValue(this, StatName.DefenseHeat, 0.5);
      effects.AddValue(this, StatName.DefenseCold, 0.5);
      effects.AddValue(this, StatName.DefenseExplosion, 0.3);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonPragmiumBear>();

      // primary loot
      lootDroplist
          .Add<ItemMeatRaw>(count: 2)
          .Add<ItemFur>(count: 2, countRandom: 1)
          .Add<ItemAnimalFat>(count: 2, countRandom: 1)
          .Add<ItemBones>(count: 2, countRandom: 1)

          .Add<ItemFishBass>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishBlackGoby>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishBlueGlider>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishBream>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishCarp>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishCatfish>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishChub>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishCrappie>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishGrunt>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishKoi>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishMackerel>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishMahimahi>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishPollock>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishPuffer>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishPyke>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishRooster>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishSalmon>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishSardine>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishTrout>(count: 1, countRandom: 1, probability: 1 / 40.0)
          .Add<ItemFishYellowfinTuna>(count: 1, countRandom: 1, probability: 1 / 40.0)

          .Add<ItemOrePragmium>(count: 5, countRandom: 1);

      // chance for extra loot (2 bonus items)
      lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                       nestedList: new DropItemsList(outputs: 2)
                                   .Add<ItemMeatRaw>(count: 1)
                                   .Add<ItemFur>(count: 1)
                                   .Add<ItemAnimalFat>(count: 1)
                                   .Add<ItemBones>(count: 1)
                                   .Add<ItemOrePragmium>(count: 2));
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobEnragedColdBearClaws>();
      data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
      data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
    }

    protected override void ServerUpdateMob(ServerUpdateData data)
    {
      var character = data.GameObject;

      ServerEnragedAiHelper.ProcessAggressiveAi(
          character,
          false,
          data.DeltaTime,
          targetStructure: ServerEnragedAiHelper.GetClosestTargetStructure(character),
          targetCharacter: ServerEnragedAiHelper.GetClosestTargetPlayer(character),
          distanceEnemyTooClose: 1.5,
          distanceAttackEnemyTooClose: 3,
          distanceEnemyTooFar: 25,
          secondsBeforeTryingGoalTarget: 20,
          secondsToAttackGoalTarget: 10,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }
  }
}