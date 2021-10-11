namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Items.Weapons;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System.Collections.Generic;

  public class ItemWeaponMobMachinegun300 : ProtoItemMobNPCWeaponRanged
  {
    public override ushort AmmoCapacity => 20;

    public override double DamageApplyDelay => 0.1;

    public override double AmmoReloadDuration => 3.0;

    public override double CharacterAnimationAimingRecoilDuration => 0.3;

    public override double CharacterAnimationAimingRecoilPower => 0.667;

    public override double CharacterAnimationAimingRecoilPowerAddCoef
        => 1 / 2.5; // full recoil power will be gained on third shot

    public override double DamageMultiplier => 0.8; // slightly lower than default

    public override string Description => "Heavy machine gun developed for high-power .300 rounds.";

    public override uint DurabilityMax => 0;

    public override double FireInterval => 0.125; // shots per second

    //public override double ReadyDelayDuration => 1 / 10d;

    public override string Name => "Heavy machine gun";

    public override double SpecialEffectProbability => 0.05;

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

    protected override WeaponFirePatternPreset PrepareFirePatternPreset()
    {
      return new(
          initialSequence: new[] { 0.0, 1.0, 2.0 },
          cycledSequence: new[] { -6, -4, -1, 0, 1.5, 3, 4, 6 });
    }

    public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
    {
      // angle variation within x degrees
      return 15 * (RandomHelper.NextDouble() - 0.5);
    }

    protected override WeaponFireTracePreset PrepareFireTracePreset()
    {
      return WeaponFireTracePresets.Heavy;
    }

    protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
    {
      description.Set(MuzzleFlashPresets.ModernSubmachinegun)
                 .Set(textureScreenOffset: (46, 7));
    }

    protected override void PrepareProtoWeaponRanged(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber300>();

      var damageDistribution = new DamageDistribution()
                               .Set(DamageType.Kinetic, 1);

      overrideDamageDescription = new DamageDescription(
          damageValue: 5,
          armorPiercingCoef: 0.7,
          finalDamageMultiplier: 1,
          rangeMax: 10,
          damageDistribution: damageDistribution);
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return WeaponsSoundPresets.WeaponRangedMachinegun;
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
    }
  }
}