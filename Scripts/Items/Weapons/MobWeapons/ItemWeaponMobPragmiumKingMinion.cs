namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System.Collections.Generic;

  public class ItemWeaponMobPragmiumKingMinion : ItemWeaponMobWeaponNovaExplosion
  {
    protected override TextureResource FXBlast => new TextureResource("FX/ExplosionBlastEnergy");

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      base.SharedOnFire(character, weaponState);

      // spawn minions after a nova attack
      MobBossPragmiumKing king = character.ProtoGameObject as MobBossPragmiumKing;
      if (king is not null)
      {

        if (mobs is null)
        {
          mobs = Api.FindProtoEntities<ProtoCharacterMob>();
          mobs.RemoveAll(m => m.StatDefaultHealthMax < 80);
          mobs.RemoveAll(m => m.AiIsRunAwayFromHeavyVehicles);
          mobs.RemoveAll(m => m.GetType().ToString().Contains("NPC"));
        }

        int r = RandomHelper.Next(mobs.Count);

        ProtoCharacterMob mob = mobs[r];

        king.ServerTrySpawnMinions(character, 6.0, 8.0, mob);

        king.ServerTrySpawnMinions(character, 10.0, 15.0, Api.GetProtoEntity<MobPsiGrove>());
      }

      return true;
    }

    static List<ProtoCharacterMob> mobs = null;

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      // no ammo used
      compatibleAmmoProtos = null;

      var damageDistribution = new DamageDistribution()
          .Set(DamageType.Cold, 1.0);

      overrideDamageDescription = new DamageDescription(
          damageValue: 10,
          armorPiercingCoef: 0.1,
          finalDamageMultiplier: 1.25,
          rangeMax: 50,
          damageDistribution: damageDistribution);
    }
  }
}