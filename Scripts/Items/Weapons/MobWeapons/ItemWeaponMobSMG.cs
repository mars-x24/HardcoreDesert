namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System.Collections.Generic;

  public class ItemWeaponMobSMG : ProtoItemMobNPCWeaponRanged
  {
    public override ushort AmmoCapacity => 5;

    public override double DamageApplyDelay => 0.05;

    public override double AmmoReloadDuration => 3.0;

    public override double CharacterAnimationAimingRecoilDuration => 0.3;

    public override double CharacterAnimationAimingRecoilPower => 0.667;

    public override double CharacterAnimationAimingRecoilPowerAddCoef
        => 1 / 2.5; // full recoil power will be gained on third shot

    public override double DamageMultiplier => 0.5;

    public override double FireAnimationDuration => 0.2;

    public override uint DurabilityMax => 0;

    public override double FireInterval => 1 / 5.0; 

    public override string Name => "Submachine gun";

    public override string CharacterAnimationAimingName => "WeaponRifleAiming";

    public virtual string CharacterAnimationAimingRecoilName => "WeaponRifleShooting";

    public override void SharedOnHit(
        WeaponFinalCache weaponCache,
        IWorldObject damagedObject,
        double damage,
        WeaponHitData hitData,
        out bool isDamageStop)
    {
      base.SharedOnHit(weaponCache,
                       damagedObject,
                       damage,
                       hitData,
                       out isDamageStop);
    }

    public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
    {
      // angle variation within x degrees
      return 15 * (RandomHelper.NextDouble() - 0.5);
    }

    protected override WeaponFirePatternPreset PrepareFirePatternPreset()
    {
      return new(
          initialSequence: new[] { 0.0, 1.0, 2.0 },
          cycledSequence: new[] { 1.5, 3.0, 2.0, 2.5 });
    }

    protected override WeaponFireTracePreset PrepareFireTracePreset()
    {
      return WeaponFireTracePresets.Firearm;
    }

    protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
    {
      description.Set(MuzzleFlashPresets.ModernSubmachinegun)
                 .Set(textureScreenOffset: (12, 19));
    }

    protected override void PrepareProtoWeaponRanged(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber10mm>();

      var damageDistribution = new DamageDistribution()
                               .Set(DamageType.Kinetic, 1);

      overrideDamageDescription = new DamageDescription(
          damageValue: 10,
          armorPiercingCoef: 0.6,
          finalDamageMultiplier: 1,
          rangeMax: 10.5,
          damageDistribution: damageDistribution);
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return WeaponsSoundPresets.WeaponRangedSMG;
    }
  }
}