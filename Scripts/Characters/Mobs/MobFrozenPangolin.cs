﻿namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesServer;

  public class MobFrozenPangolin : ProtoCharacterMob
  {
    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override bool CanBeSpawnedByMobs => false;

    public override float CharacterWorldHeight => 0.9f;

    public override double MobKillExperienceMultiplier => 2.0;

    public override string Name => "Frozen Pangolin";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Glass;

    public override double StatDefaultHealthMax => 400;

    public override double StatMoveSpeed => 0.8;

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 1.0);
      effects.AddValue(this, StatName.DefenseKinetic, 1.0);
      effects.AddValue(this, StatName.DefenseCold, 1.0);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonFrozenPangolin>();

      // primary loot
      lootDroplist
          .Add<ItemAnimalFat>(count: 2)
          .Add<ItemBones>(count: 2, countRandom: 1)
          .Add<ItemOrePragmium>(count: 2);

      // random loot
      lootDroplist
          .Add<ItemMeatRaw>(count: 2, probability: 1 / 3.0)
          .Add<ItemOrePragmium>(count: 1, probability: 1 / 3.0);

      // extra loot
      lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                       nestedList: new DropItemsList(outputs: 1)
                                   .Add<ItemAnimalFat>(count: 1)
                                   .Add<ItemBones>(count: 1)
                                   .Add<ItemOrePragmium>(count: 1, probability: 1 / 3.0));
    }
    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobLizardCold>();
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
          distanceEnemyTooFar: 10,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);

      IWorldServerService ServerWorldService = Api.IsServer ? Api.Server.World : null;
    }

  }
}