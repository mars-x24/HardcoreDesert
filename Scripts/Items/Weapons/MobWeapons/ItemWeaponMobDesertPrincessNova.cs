namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Resources;

  public class ItemWeaponMobDesertPrincessNova : ItemWeaponMobWeaponNovaExplosion
  {
    protected override TextureResource FXBlast => new TextureResource("FX/ExplosionBlastPrincess");

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      base.SharedOnFire(character, weaponState);

      // spawn minions after a nova attack
      (character.ProtoGameObject as MobDesertPrincess)
          ?.ServerTrySpawnMinions(character);

      return true;
    }

  }
}