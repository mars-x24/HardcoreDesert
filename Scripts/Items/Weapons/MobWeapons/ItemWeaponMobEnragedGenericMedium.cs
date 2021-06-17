namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using System.Collections.Generic;

  public class ItemWeaponMobEnragedGenericMedium : ItemWeaponMobGenericMedium
  {
    public override bool CanDamageStructures => true;

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      return base.SharedOnFire(character, weaponState);
    }

    public override void SharedOnHit(WeaponFinalCache weaponCache, IWorldObject damagedObject, double damage, WeaponHitData hitData, out bool isDamageStop)
    {
      weaponCache.AllowNpcToNpcDamage = true;
      
      base.SharedOnHit(weaponCache, damagedObject, damage, hitData, out isDamageStop);
    }

    protected override void PrepareProtoWeapon(
    out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
    ref DamageDescription overrideDamageDescription)
    {
      // no ammo used
      compatibleAmmoProtos = null;

      overrideDamageDescription = new DamageDescription(
          damageValue: 30,
          armorPiercingCoef: 0.5,
          finalDamageMultiplier: 1,
          rangeMax: 2.0,
          damageDistribution: new DamageDistribution().Set(DamageType.Impact, 1.0));
    }
  }
}