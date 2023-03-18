using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
using AtomicTorch.CBND.CoreMod.Items.Ammo;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Physics;
using AtomicTorch.CBND.GameApi.Data.Weapons;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;
using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using System.Collections.Generic;
using System.Windows.Media;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  public class ItemWeaponMobEnragedFloaterNova : ProtoItemMobWeaponNova
  {
    public override bool CanDamageStructures => true;

    public override double DamageApplyDelay => 0.4;

    public override double FireAnimationDuration => 1.0;

    public override double FireInterval => 2.0;

    public override (float min, float max) SoundPresetWeaponDistance
        => (6, 45);

    public override string WeaponAttachmentName => "Head";

    public override void ClientOnWeaponShot(ICharacter character)
    {
      ClientTimersSystem.AddAction(this.DamageApplyDelay - 0.11667,
                                   () => this.ClientCreateBlastwave(character));
    }

    public override string GetCharacterAnimationNameFire(ICharacter character)
    {
      return "Attack";
    }

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      // no ammo used
      compatibleAmmoProtos = null;

      var damageDistribution = new DamageDistribution()
          .Set(DamageType.Chemical, 1.0);

      overrideDamageDescription = new DamageDescription(
          damageValue: 60,
          armorPiercingCoef: 1.0,
          finalDamageMultiplier: 1,
          rangeMax: 3.0,
          damageDistribution: damageDistribution);
    }

    public override void SharedOnHit(WeaponFinalCache weaponCache, IWorldObject damagedObject, double damage, WeaponHitData hitData, out bool isDamageStop)
    {
      weaponCache.AllowNpcToNpcDamage = true;

      base.SharedOnHit(weaponCache, damagedObject, damage, hitData, out isDamageStop);
    }

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      if (IsClient)
      {
        return true;
      }

      var damageRadius = this.OverrideDamageDescription.RangeMax;

      WeaponExplosionSystem.ServerProcessExplosionCircle(
          positionEpicenter: character.Position + (0, this.SharedGetFireYOffset(character)),
          physicsSpace: Server.World.GetPhysicsSpace(),
          damageDistanceMax: damageRadius,
          weaponFinalCache: weaponState.WeaponCache,
          damageOnlyDynamicObjects: false,
          isDamageThroughObstacles: true,
          callbackCalculateDamageCoefByDistanceForStaticObjects:
          this.ServerCalculateDamageCoefficientByDistanceToTarget,
          callbackCalculateDamageCoefByDistanceForDynamicObjects:
          this.ServerCalculateDamageCoefficientByDistanceToTarget,
          collisionGroups: new[]
          {
                    CollisionGroups.HitboxMelee,
                    CollisionGroups.HitboxRanged,
                    CollisionGroup.Default
          });

      return true;
    }

    private void ClientCreateBlastwave(ICharacter character)
    {
      // add blast wave
      var rangeMax = 4 + 2 * this.OverrideDamageDescription.RangeMax;

      const double blastAnimationDuration = 0.6;
      var blastWaveColor = Color.FromRgb(0xCC, 0xEE, 0xAA);
      var blastSize = new Size2F(rangeMax, rangeMax);
      var blastwaveSizeFrom = 2 * new Size2F(128, 128);
      var blastwaveSizeTo = 128 * blastSize;

      var blastSceneObject = Client.Scene.CreateSceneObject(
          "Temp Nova",
          character.Position + (0, this.SharedGetFireYOffset(character)));

      blastSceneObject.Destroy(delay: blastAnimationDuration);

      var blastSpriteRenderer = Client.Rendering.CreateSpriteRenderer(blastSceneObject,
                                                                      new TextureResource("FX/ExplosionBlast"),
                                                                      drawOrder: DrawOrder.Light,
                                                                      spritePivotPoint: (0.5, 0.5));
      blastSpriteRenderer.BlendMode = BlendMode.AlphaBlendPremultiplied;

      // animate blast wave
      ClientComponentGenericAnimationHelper.Setup(
          blastSceneObject,
          blastAnimationDuration,
          updateCallback: alpha =>
                          {
                            var blastwaveAlpha = (byte)(byte.MaxValue * (1 - alpha));
                            blastSpriteRenderer.Color = blastWaveColor.WithAlpha(blastwaveAlpha);

                            var sizeX = MathHelper.Lerp(blastwaveSizeFrom.X,
                                                              blastwaveSizeTo.X,
                                                              alpha);
                            var sizeY = MathHelper.Lerp(blastwaveSizeFrom.Y,
                                                              blastwaveSizeTo.Y,
                                                              alpha);
                            blastSpriteRenderer.Size = (sizeX, sizeY);
                          });
    }
  }
}