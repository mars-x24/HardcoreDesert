namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using System.Collections.Generic;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.GameEngine.Common.Helpers;

  public class ItemWeaponMobMutantCrawlerPoison : ProtoItemMobWeaponRanged
  {
    public override ushort AmmoCapacity => 0;

    public override double AmmoReloadDuration => 2.0; // lower

    public override double CharacterAnimationAimingRecoilDuration => 0.4;

    public override double CharacterAnimationAimingRecoilPower => 0.667;

    public override double DamageApplyDelay => 0.2;

    public override double DamageMultiplier => 1.0;

    public override double FireAnimationDuration => 0.3;

    public override uint DurabilityMax => 0;

    public override bool IsLoopedAttackAnimation => false;

    public override double FireInterval => 1.75;

    public override double SpecialEffectProbability => 0.3;

    //public override string Name => "Handgun";

    public override double ReadyDelayDuration => WeaponReadyDelays.ConventionalPistols;

    public override string CharacterAnimationAimingName => null;

    public override string CharacterAnimationAimingRecoilName => null;

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
      return 25 * (RandomHelper.NextDouble() - 0.5);
    }

    protected override WeaponFireTracePreset PrepareFireTracePreset()
    {
      return WeaponFireTracePresets.MobPoison;
    }

    protected override void PrepareProtoWeaponRanged(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      compatibleAmmoProtos = null;

      var damageDistribution = new DamageDistribution()
                               .Set(DamageType.Kinetic, 0.7)
                               .Set(DamageType.Chemical, 0.3);

      overrideDamageDescription = new DamageDescription(
          damageValue: 5,
          armorPiercingCoef: 0.2,
          finalDamageMultiplier: 1.25,
          rangeMax: 3,
          damageDistribution: damageDistribution);
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      if (RandomHelper.RollWithProbability(0.7))
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.2);
      }
    }

    public override (float min, float max) SoundPresetWeaponDistance
        => (SoundConstants.AudioListenerMinDistanceRangedShot + 3,
            SoundConstants.AudioListenerMaxDistanceRangedShotMobs + 8);
  }
}