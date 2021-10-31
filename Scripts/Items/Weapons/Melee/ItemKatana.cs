namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.Base;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using System.Collections.Generic;

  public class ItemKatana : ProtoItemWeaponMelee, IProtoPiercingWeapon
  {
    public override double DamageApplyDelay => 0.075;

    public override string Description => "Single-edged sword that can slice through tough materials.";

    public override uint DurabilityMax => 230;

    public override double FireAnimationDuration => 0.6;

    public override string Name => "Katana";

    public override double SpecialEffectProbability => 0.3; // 30%

    protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      compatibleAmmoProtos = null;

      overrideDamageDescription = new DamageDescription(
          damageValue: 40,
          armorPiercingCoef: 0.5,
          finalDamageMultiplier: 1.2,
          rangeMax: 1.2,
          damageDistribution: new DamageDistribution(DamageType.Impact, 1));
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      ServerWeaponSpecialEffectsHelper.OnPickaxeHit(damagedCharacter, damage);
    }
  }
}