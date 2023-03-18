using AtomicTorch.CBND.CoreMod.Characters.Mobs;
using AtomicTorch.CBND.CoreMod.Items.Ammo;
using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Weapons;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.GameEngine.Common.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  public class ItemWeaponMobPragmiumKingNova : ItemWeaponMobWeaponNovaExplosion
  {
    protected new const double NovaBigDamageRange = 4.0;

    protected override TextureResource FXBlast => new TextureResource("FX/ExplosionBlastKing");

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      base.SharedOnFire(character, weaponState);

      DestroySalt(character.TilePosition.ToVector2D(), 20);

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

    public static void DestroySalt(Vector2D circlePosition, ushort circleRadius)
    {
      int size = circleRadius * 2;
      var rect = new RectangleInt((int)circlePosition.X - circleRadius, (int)circlePosition.Y - circleRadius, size, size);

      var list = Server.World.GetStaticWorldObjectsOfProtoInBounds<ObjectMineralSalt>(rect).ToList();

      foreach (var obj in list)
      {
        if (!obj.IsDestroyed)
        {
          Server.World.DestroyObject(obj);
        }
      }
    }
  }
}