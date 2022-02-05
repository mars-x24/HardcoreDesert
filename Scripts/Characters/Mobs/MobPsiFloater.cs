namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Objects;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.GameApi.Data.World;

  public class MobPsiFloater : ProtoCharacterMob, IProtoObjectPsiSource
  {
    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override double BiomaterialValueMultiplier => 2.0;

    public override float CharacterWorldHeight => 2.4f;

    public override float CharacterWorldWeaponOffsetRanged => 1.1f;

    public override double MobKillExperienceMultiplier => 5.0;

    public override string Name => "Psi floater";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

    public override double PhysicsBodyAccelerationCoef => 1;

    public override double PhysicsBodyFriction => 0.5;

    public override double StatDefaultHealthMax => 450;

    public override double StatMoveSpeed => 2.5;

    public double PsiIntensity => 0.9;

    public double PsiRadiusMax => 4;

    public double PsiRadiusMin => 2;

    public bool ServerIsPsiSourceActive(IWorldObject worldObject)
    {
      return true;
    }

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      // easy to kill by normal means, but have very tough protection against certain types of damage
      effects.AddValue(this, StatName.DefenseImpact, 1.0)
             .AddValue(this, StatName.DefenseKinetic, 0.2)
             .AddValue(this, StatName.DefenseHeat, 0.2)
             .AddValue(this, StatName.DefenseChemical, 0.9)
             .AddValue(this, StatName.DefensePsi, 1.0)
             .AddValue(this, StatName.DefenseRadiation, 0.9);
    }

    protected override void ClientOnSpawnAnimationStateChanged(GameApi.Data.Characters.ICharacter character)
    {
      base.ClientOnSpawnAnimationStateChanged(character);
    }

    protected override void PrepareProtoCharacterMob(
        out ProtoCharacterSkeleton skeleton,
        ref double scale,
        DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<SkeletonPsiFloater>();

      // primary loot
      lootDroplist
          .Add<ItemSlime>(count: 10, countRandom: 10)
          .Add<ItemSalt>(count: 5, countRandom: 5)
          .Add<ItemRubberRaw>(count: 8, countRandom: 4)
          .Add<ItemKeinite>(count: 2, countRandom: 2)
          .Add<ItemAlienBrain>(count: 1);

      // extra loot
      lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                       nestedList: new DropItemsList(outputs: 2)
                                   .Add<ItemSlime>(count: 5)
                                   .Add<ItemSalt>(count: 2))
                                   .Add<ItemRubberRaw>(count: 2);
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobFloaterNova>();
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
          distanceEnemyTooFar: 15,
          movementDirection: out var movementDirection,
          rotationAngleRad: out var rotationAngleRad);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }
  }
}