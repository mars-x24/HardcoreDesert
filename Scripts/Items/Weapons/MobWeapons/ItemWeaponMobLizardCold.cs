namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System.Collections.Generic;

  public class ItemWeaponMobLizardCold : ProtoItemMobWeaponRangedNoAim
  {
    public override double DamageApplyDelay => 0.5;

    public override double FireAnimationDuration => 1.5;

    public override double FireInterval => 2.5;

    public override string WeaponAttachmentName => "Head";

    public override string GetCharacterAnimationNameFire(ICharacter character)
    {
      return GetMeleeCharacterAnimationNameFire(character);
    }

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

      if (IsServer
          && damage > 0
          && damagedObject is ICharacter damagedCharacter)
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectCold>(intensity: 0.5);
        damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(intensity: 1.0);
        //damagedCharacter.ServerAddStatusEffect<StatusEffectWeakened>(intensity: 0.2);
      }
    }

    public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
    {
      // angle variation within 30 degrees
      return 30 * (RandomHelper.NextDouble() - 0.5);
    }

    protected override WeaponFireTracePreset PrepareFireTracePreset()
    {
      return MobCold;
    }

    public static readonly WeaponFireTracePreset MobCold
      = new(traceTexturePath: "FX/WeaponTraces/TraceCold",
            hitSparksPreset: WeaponHitSparksPresets.LaserBlue,
            traceSpeed: 20,
            traceSpriteWidthPixels: 362,
            traceStartScaleSpeedExponent: 0.5,
            traceStartOffsetPixels: -17);

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      // no ammo used
      compatibleAmmoProtos = null;

      var damageDistribution = new DamageDistribution()
                               .Set(DamageType.Cold, 0.6)
                               .Set(DamageType.Impact, 0.4);

      overrideDamageDescription = new DamageDescription(
          damageValue: 8,
          armorPiercingCoef: 0.4,
          finalDamageMultiplier: 1.25,
          rangeMax: 8,
          damageDistribution: damageDistribution);
    }
  }
}