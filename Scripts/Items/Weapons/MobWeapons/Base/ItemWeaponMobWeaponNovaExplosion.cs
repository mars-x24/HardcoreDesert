namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
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
  using System;
  using System.Collections.Generic;
  using System.Windows.Media;


  public class ItemWeaponMobWeaponNovaExplosion : ProtoItemMobWeaponNova
  {
    public override double DamageApplyDelay => 1.05;

    public override double FireAnimationDuration => 1.3;

    public override double FireInterval => 3.0;

    public override (float min, float max) SoundPresetWeaponDistance
        => (15, 45);

    public override (float min, float max) SoundPresetWeaponDistance3DSpread
        => (10, 35);

    public override string WeaponAttachmentName => "Head";

    protected virtual TextureResource FXBlast => new TextureResource("FX/ExplosionBlast");

    public override void ClientOnWeaponShot(ICharacter character)
    {
      ClientTimersSystem.AddAction(this.DamageApplyDelay - 0.11667,
                                   () => this.ClientCreateBlastwave(character));

      ClientTimersSystem.AddAction(this.DamageApplyDelay - 0.11667,
                                   ClientCreateScreenShakes);
    }

    public virtual double NovaBigDamageRange => 3.0;

    public override void ServerOnObjectHitByNova(
        IWorldObject damagedObject,
        double damageApplied,
        WeaponFinalCache weaponFinalCache)
    {
      // daze damaged players
      if (damagedObject is ICharacter character
          && !character.IsNpc
          && damageApplied > 1)
      {
        character.ServerAddStatusEffect<StatusEffectDazed>(intensity: 1.0);
      }

      double distance = 0.0;
      if (characterOnFire is not null)
        distance = characterOnFire.TilePosition.TileDistanceTo(damagedObject.TilePosition);

      if (Math.Abs(distance) <= NovaBigDamageRange)
      {
        DamageDescription damage = new DamageDescription(200.0, 1.0, 1.0, 1.0, new DamageDistribution(DamageType.Kinetic, 1.0));
        WeaponFinalCache weapon = new WeaponFinalCache(null, Stats.FinalStatsCache.Empty, null, null, null, damage, null, null, true);

        // close npc
        if (damagedObject is IStaticWorldObject && damagedObject is IDamageableProtoWorldObject obj)
        {
          obj.SharedOnDamage(weapon, damagedObject, 1.0, 1.0, out double dc, out double da);
        }

        // close mob
        if (damagedObject.ProtoGameObject is ProtoCharacterMob mob)
        {
          if (mob.StatMoveSpeed == 0)
          {
            mob.SharedOnDamage(weapon, damagedObject, 1.0, 1.0, out double dc, out double da);
          }
        }
      }
    }

    private ICharacter characterOnFire = null;

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      this.characterOnFire = character;

      base.SharedOnFire(character, weaponState);


      //SharedExplosionHelper.ServerExplode(
      //         character: null,
      //         protoExplosive: null,
      //         protoWeapon: null,
      //         explosionPreset: ClearExplosion,
      //         epicenterPosition: character.TilePosition.ToVector2D(),
      //         damageDescriptionCharacters: ClearExplosionDamage,
      //         physicsSpace: Server.World.GetPhysicsSpace(),
      //         executeExplosionCallback: this.ServerExecuteExplosion,
      //         allowNpcToNpcDamage: true);


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
          damageValue: 20,
          armorPiercingCoef: 0.1,
          finalDamageMultiplier: 1.25,
          rangeMax: 10,
          damageDistribution: damageDistribution);
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return new SoundPreset<WeaponSound>(customDistance: (15, 45),
                                          customDistance3DSpread: (10, 35))
          .Add(WeaponSound.Shot, "Skeletons/PragmiumQueen/Weapon/ShotNova");
    }

    protected override float SharedGetFireYOffset(ICharacter character)
    {
      return 0;
    }

    private static void ClientCreateScreenShakes()
    {
      const float shakesDuration = 0.4f,
                  shakesDistanceMin = 0.9f,
                  shakesDistanceMax = 1.1f;
      ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                        worldDistanceMin: -shakesDistanceMin,
                                                        worldDistanceMax: shakesDistanceMax);
    }

    public virtual void ServerExecuteExplosion(
   Vector2D positionEpicenter,
   IPhysicsSpace physicsSpace,
   WeaponFinalCache weaponFinalCache)
    {
      WeaponExplosionSystem.ServerProcessExplosionCircle(
          positionEpicenter: positionEpicenter,
          physicsSpace: physicsSpace,
          damageDistanceMax: weaponFinalCache.RangeMax,
          weaponFinalCache: weaponFinalCache,
          damageOnlyDynamicObjects: false,
          isDamageThroughObstacles: true,
          callbackCalculateDamageCoefByDistanceForStaticObjects:
          this.ServerCalculateDamageCoefByDistanceForStaticObjects,
          callbackCalculateDamageCoefByDistanceForDynamicObjects:
          this.ServerCalculateDamageCoefByDistanceForDynamicObjects);
    }

    public virtual double ServerCalculateDamageCoefByDistanceForDynamicObjects(double distance)
    {
      return 1;
    }

    public virtual double ServerCalculateDamageCoefByDistanceForStaticObjects(double distance)
    {
      return 1;
    }

    protected virtual DamageDescription ClearExplosionDamage => new DamageDescription(
                damageValue: 600,
                armorPiercingCoef: 0.0,
                finalDamageMultiplier: 1,
                rangeMax: 3,
                damageDistribution: new DamageDistribution().Set(DamageType.Kinetic, 1.0));

    public static readonly ExplosionPreset ClearExplosion
        = ExplosionPreset.CreatePreset(
            protoObjectCharredGround: null,
            serverDamageApplyDelay: 0.8 * 0.25,
            soundSetPath: "Explosions/ExplosionSmall",
            spriteAnimationDuration: 0.8,
            spriteSetPath: "FX/Explosions/ExplosionLarge2",
            spriteAtlasColumns: 8,
            spriteAtlasRows: 3,
            spriteWorldSize: new Size2F(1.5, 1.5),
            blastwaveDelay: 0.05,
            blastwaveAnimationDuration: 0.4,
            blastWaveColor: Color.FromRgb(0xFF, 0xBB, 0x33),
            blastwaveWorldSizeFrom: 0.25 * new Size2F(3, 2),
            blastwaveWorldSizeTo: 1 * new Size2F(3, 2),
            lightDuration: 1,
            lightWorldSize: 35,
            lightColor: Color.FromRgb(0xFF, 0xCC, 0x66),
            screenShakesDuration: 0.2,
            screenShakesWorldDistanceMin: 0.15,
            screenShakesWorldDistanceMax: 0.2,
            spriteColorAdditive: Color.FromRgb(0x22, 0x22, 0x00),
            spriteColorMultiplicative: Color.FromRgb(0xFF, 0xEE, 0xAA),
            spriteBrightness: 1.33,
            spriteDrawOrder: DrawOrder.Light + 1,
            soundsCuesNumber: 6);

    protected virtual void ClientCreateBlastwave(ICharacter character)
    {
      // add blast wave
      var rangeMax = 4 + 2 * this.OverrideDamageDescription.RangeMax;

      const double blastAnimationDuration = 1.0;
      var blastWaveColor = Color.FromRgb(0x99, 0xDD, 0xFF);
      var blastSize = new Size2F(rangeMax, rangeMax);
      var blastwaveSizeFrom = 2 * new Size2F(128, 128);
      var blastwaveSizeTo = 128 * blastSize;

      var blastSceneObject = Client.Scene.CreateSceneObject(
          "Temp Nova",
          character.Position + (0, this.SharedGetFireYOffset(character)));

      blastSceneObject.Destroy(delay: blastAnimationDuration);

      var blastSpriteRenderer = Client.Rendering.CreateSpriteRenderer(blastSceneObject,
                                                                      this.FXBlast,
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
