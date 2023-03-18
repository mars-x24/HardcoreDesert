using AtomicTorch.CBND.CoreMod.Items.Ammo;
using AtomicTorch.CBND.CoreMod.Items.Weapons.Base;
using AtomicTorch.CBND.CoreMod.Skills;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Weapons;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
  public class ItemSwordSteel : ProtoItemWeaponMelee, IProtoPiercingWeapon
  {
    public override double DamageApplyDelay => 0.075;

    public override string Description => "Steel longsword, your choice in any confusing situation!";

    public override uint DurabilityMax => 200;

    public override double FireAnimationDuration => 0.8;

    public override string Name => "Steel sword";

    public override double SpecialEffectProbability => 0.35; // 35%

    protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      compatibleAmmoProtos = null;

      overrideDamageDescription = new DamageDescription(
          damageValue: 50,
          armorPiercingCoef: 0.3,
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