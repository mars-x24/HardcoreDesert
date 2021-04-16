namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Resources;
  using System.Collections.Generic;

  public class ItemWeaponMobPragmiumKingNova : ItemWeaponMobWeaponNovaExplosion
  {
    protected new const double NovaBigDamageRange = 4.0;

    protected override TextureResource FXBlast => new TextureResource("FX/ExplosionBlastKing");

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      base.SharedOnFire(character, weaponState);

      // spawn minions after a nova attack
      (character.ProtoGameObject as MobBossPragmiumKing)
          ?.ServerTrySpawnMinions(character);

      return true;
    }

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      // no ammo used
      compatibleAmmoProtos = null;

      var damageDistribution = new DamageDistribution()
          .Set(DamageType.Cold, 1.0);

      overrideDamageDescription = new DamageDescription(
          damageValue: 40,
          armorPiercingCoef: 0.6,
          finalDamageMultiplier: 1.25,
          rangeMax: 50,
          damageDistribution: damageDistribution);
    }

    protected override DamageDescription ClearExplosionDamage => new DamageDescription(
                damageValue: 600,
                armorPiercingCoef: 0.0,
                finalDamageMultiplier: 1,
                rangeMax: 4,
                damageDistribution: new DamageDistribution().Set(DamageType.Kinetic, 1.0));

  }
}