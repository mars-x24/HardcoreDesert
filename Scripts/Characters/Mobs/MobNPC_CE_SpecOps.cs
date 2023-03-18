using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
using AtomicTorch.CBND.CoreMod.Items.Ammo;
using AtomicTorch.CBND.CoreMod.Items.Devices;
using AtomicTorch.CBND.CoreMod.Items.Equipment;
using AtomicTorch.CBND.CoreMod.Items.Medical;
using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.Stats;
using AtomicTorch.CBND.CoreMod.Systems.Droplists;
using AtomicTorch.CBND.GameApi.Data.Characters;

namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
  public class MobNPC_CE_SpecOps : ProtoCharacterRangedNPC
  {
    public override string Name => "Spec Ops";

    public override bool CanBeSpawnedByMobs => false;

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

    public override float CharacterWorldHeight => 1.5f;

    public override float CharacterWorldWeaponOffsetRanged => 0.4f;

    public override double StatMoveSpeed => 2.0;

    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override double MobKillExperienceMultiplier => 2.0;

    public override double StatDefaultHealthMax => 300;

    public override double StatHealthRegenerationPerSecond => 5.0 / 5.0; // Default: 10/60 10 health points per minute

    public override bool IsAvailableInCompletionist => false;

    public override double RetreatDistance => 5;

    public override double EnemyToCloseDistance => 9;

    public override double EnemyToFarDistance => 14;

    public override bool RetreatWhenReloading => true;

    public override double RetreatingHealthPercentage => 60.0;


    protected override void FillDefaultEffects(Effects effects)
    {
      base.FillDefaultEffects(effects);

      effects.AddValue(this, StatName.DefenseImpact, 0.7);
      effects.AddValue(this, StatName.DefenseKinetic, 1.0);
      effects.AddValue(this, StatName.DefenseExplosion, 0.5);
      effects.AddValue(this, StatName.DefenseChemical, 0.5);
    }

    protected override void PrepareProtoCharacterMob(
           out ProtoCharacterSkeleton skeleton,
           ref double scale,
           DropItemsList lootDroplist)
    {
      skeleton = GetProtoEntity<CharacterSkeletons.SkeletonNPC_CE_SpecOps>();

      // primary loot
      lootDroplist.Add(
        new DropItemsList(outputs: 1, outputsRandom: 2)

          .Add<ItemAmmo300ArmorPiercing>(count: 18, countRandom: 36, weight: 15)
          .Add<ItemAntiToxin>(count: 1, countRandom: 1, weight: 5)
          .Add<ItemAntiToxinPreExposure>(count: 1, weight: 5)
          .Add<ItemMedkit>(count: 1, countRandom: 1, weight: 1)
          .Add<ItemLaserSight>(count: 1, weight: 1)
          .Add<ItemPowerBankLarge>(count: 1, weight: 1));

      // extra loot from skill
      lootDroplist.Add(
          probability: 1 / 20.0,
          nestedList:
        new DropItemsList(outputs: 1)

          .Add<ItemAssaultHelmet>(count: 1, weight: 1)
          .Add<ItemAssaultArmor>(count: 1, weight: 1)
          .Add<ItemMachinegun300>(count: 1, weight: 1));
    }

    //TODO: this NPC could shot and drop Toxic .300 ammo
    protected override void ServerInitializeCharacterMob(ServerInitializeData data)
    {
      base.ServerInitializeCharacterMob(data);

      var weaponProto = GetProtoEntity<ItemWeaponMobMachinegun300>();
      data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
      data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);

    }

    protected override void ServerOnAggro(ICharacter characterMob, ICharacter characterToAggro)
    {
      // cannot auto-aggro
    }

  }
}