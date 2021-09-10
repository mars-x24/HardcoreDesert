namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Medical;
  using AtomicTorch.CBND.CoreMod.Items.Equipment;
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.CoreMod.Objects;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Stats;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.GameApi.Data.World;

  public class MobNPC_BA_Specialist : ProtoCharacterRangedNPC
  {

    public override string Name => "Specialist";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

    public override float CharacterWorldHeight => 1.5f;

    public override float CharacterWorldWeaponOffsetRanged => 0.4f;

    public override double StatMoveSpeed => 2.0;

    public override double StatMoveSpeedRunMultiplier => 1.1;

    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override double MobKillExperienceMultiplier => 1.0;

    public override double StatDefaultHealthMax => 220;

    public override double StatHealthRegenerationPerSecond => 20.0 / 20.0; // Default: 10/60 10 health points per minute

    public override bool IsAvailableInCompletionist => false;

    public override double RetreatDistance => 5;

    public override double EnemyToCloseDistance => 9;

    public override double EnemyToFarDistance => 14;

    public override bool RetreatWhenReloading => true;

    public override double RetreatingHealthPercentage => 60.0;

    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 0.5);
      effects.AddValue(this, StatName.DefenseKinetic, 0.65);
      effects.AddValue(this, StatName.DefenseExplosion, 0.4);
      effects.AddValue(this, StatName.DefenseChemical, 0.3);

    }

    protected override void PrepareProtoCharacterMob(
           out ProtoCharacterSkeleton skeleton,
           ref double scale,
           DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<CharacterSkeletons.NPC_BA_Specialist>();

      // primary loot
      lootDroplist.Add(
        new DropItemsList(outputs: 1, outputsRandom: 1)

          .Add<ItemAmmo10mmArmorPiercing>(count: 20, countRandom: 30, weight: 12)
          .Add<ItemBandage>(count: 1, countRandom: 1, weight: 7)
          .Add<ItemCigarettes>(count: 1, countRandom: 1, weight: 6)
          .Add<ItemMRE>(count: 1, weight: 2)
          .Add<ItemMedkit>(count: 1, weight: 1));

      // extra loot from skill
      lootDroplist.Add(
          condition: SkillSearching.ServerRollExtraLoot,
          nestedList:
          new DropItemsList(outputs: 1)

    .Add<ItemMilitaryArmor>(count: 1, weight: 1)
    .Add<ItemSubmachinegun10mm>(count: 1, weight: 1));
    }

    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobSMG>();
      data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
      data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);

    }

    protected override void ServerOnAggro(ICharacter characterMob, ICharacter characterToAggro)
    {
      // cannot auto-aggro
    }

  }
}