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

  public class MobPragmiumBear : ProtoCharacterMob
  {
    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override float CharacterWorldHeight => 1.8f;

    public override float CharacterWorldWeaponOffsetRanged => 0.45f;

    public override double CorpseInteractionAreaScale => 1.25;

    public override double MobKillExperienceMultiplier => 2.0;

    public override string Name => "Pragmium Bear";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

    public override double StatDefaultHealthMax => 600;

    public override double StatMoveSpeed => 2.0;

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 1.0);
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

      var weaponProto = GetProtoEntity<ItemWeaponMobColdBearClaws>();
      data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
      data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
    }

    protected override void ServerUpdateMob(ServerUpdateData data)
    {
      var character = data.GameObject;

      ServerCharacterAiHelper.ProcessAggressiveAi(
          character,
          targetCharacter: ServerCharacterAiHelper.GetClosestTargetPlayer(character),
          isRetreating: false,
          isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
          distanceRetreat: 0,
          distanceEnemyTooClose: 1,
          distanceEnemyTooFar: 6,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }
  }
}