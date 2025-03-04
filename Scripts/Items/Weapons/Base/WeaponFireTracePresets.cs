﻿namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
  public static class WeaponFireTracePresets
  {
    public static readonly WeaponFireTracePreset Arrow
        = new(traceTexturePath: "FX/WeaponTraces/TraceArrow",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 19,
              traceSpriteWidthPixels: 250,
              traceStartOffsetPixels: -10);

    public static readonly WeaponFireTracePreset Artillery
        = new(traceTexturePath: "FX/WeaponTraces/TraceArtillery",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 25,
              traceSpriteWidthPixels: 190,
              traceStartOffsetPixels: 10);

    public static readonly WeaponFireTracePreset Blackpowder
        = new(traceTexturePath: "FX/WeaponTraces/TraceBlackpowder",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 17,
              traceSpriteWidthPixels: 363,
              traceStartOffsetPixels: -10);

    public static readonly WeaponFireTracePreset ExoticWeaponPoison
        = new(traceTexturePath: "FX/WeaponTraces/TracePoison",
              hitSparksPreset: WeaponHitSparksPresets.Toxin,
              traceSpeed: 20,
              traceSpriteWidthPixels: 362,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -17);

    public static readonly WeaponFireTracePreset ExoticWeaponPoisonBig
        = new(traceTexturePath: "FX/WeaponTraces/TracePoisonBig",
              hitSparksPreset: WeaponHitSparksPresets.Toxin,
              traceSpeed: 20,
              traceSpriteWidthPixels: 514,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -50);

    public static readonly WeaponFireTracePreset Firearm
        = new(traceTexturePath: "FX/WeaponTraces/TraceFirearm",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 20,
              traceSpriteWidthPixels: 363,
              traceStartOffsetPixels: -10);

    public static readonly WeaponFireTracePreset Grenade
        = new(traceTexturePath: "FX/WeaponTraces/TraceGrenade",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 20,
              traceSpriteWidthPixels: 169,
              traceStartOffsetPixels: -10);

    public static readonly WeaponFireTracePreset Heavy
        = new(traceTexturePath: "FX/WeaponTraces/TraceHeavy",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 20,
              traceSpriteWidthPixels: 363,
              traceStartOffsetPixels: -10);

    public static readonly WeaponFireTracePreset HeavySniper
        = new(traceTexturePath: "FX/WeaponTraces/TraceHeavy",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 30,
              traceSpriteWidthPixels: 363,
              traceStartOffsetPixels: -10);

    // beam without a projectile texture
    public static readonly WeaponFireTracePreset LaserBeamBlue
        = new(traceTexturePath: null,
              hitSparksPreset: WeaponHitSparksPresets.LaserBlue,
              // not used
              traceSpeed: 1,
              traceSpriteWidthPixels: 1,
              traceStartScaleSpeedExponent: 0,
              traceStartOffsetPixels: 0,
              useScreenBlending: true,
              drawHitSparksAsLight: true);

    // beam without a projectile texture
    public static readonly WeaponFireTracePreset LaserBeamRed
            = new(traceTexturePath: null,
                  hitSparksPreset: WeaponHitSparksPresets.LaserRed,
                  // not used
                  traceSpeed: 1,
                  traceSpriteWidthPixels: 1,
                  traceStartScaleSpeedExponent: 0,
                  traceStartOffsetPixels: 0,
                  useScreenBlending: true,
                  drawHitSparksAsLight: true);

    public static readonly WeaponFireTracePreset LaserBlue
        = new(traceTexturePath: "FX/WeaponTraces/TraceLaserBlue",
              hitSparksPreset: WeaponHitSparksPresets.LaserBlue,
              traceSpeed: 25,
              traceSpriteWidthPixels: 363,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -10,
              useScreenBlending: true,
              drawHitSparksAsLight: true);

    public static readonly WeaponFireTracePreset LaserRed
        = new(traceTexturePath: "FX/WeaponTraces/TraceLaserRed",
              hitSparksPreset: WeaponHitSparksPresets.LaserRed,
              traceSpeed: 25,
              traceSpriteWidthPixels: 363,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -10,
              useScreenBlending: true,
              drawHitSparksAsLight: true);

    public static readonly WeaponFireTracePreset MeleeWeapon
        = new(traceTexturePath: null,
              hitSparksPreset: WeaponHitSparksPresets.NoWeapon,
              traceSpeed: 1,
              traceSpriteWidthPixels: 0,
              traceStartOffsetPixels: 0);

    public static readonly WeaponFireTracePreset MobPoison
        = new(traceTexturePath: "FX/WeaponTraces/TracePoison",
              hitSparksPreset: WeaponHitSparksPresets.Toxin,
              traceSpeed: 20,
              traceSpriteWidthPixels: 362,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -17);

    public static readonly WeaponFireTracePreset MobPoisonBig
        = new(traceTexturePath: "FX/WeaponTraces/TracePoisonBig",
              hitSparksPreset: WeaponHitSparksPresets.Toxin,
              traceSpeed: 35,
              traceSpriteWidthPixels: 514,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -50);

    public static readonly WeaponFireTracePreset MobPragmiumQueen
        = new(traceTexturePath: "FX/WeaponTraces/TraceMobWeaponPragmiumQueen",
              hitSparksPreset: WeaponHitSparksPresets.Plasma,
              traceSpeed: 20,
              traceSpriteWidthPixels: 362,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -33);

    public static readonly WeaponFireTracePreset NoWeapon
        = new(traceTexturePath: null,
              hitSparksPreset: WeaponHitSparksPresets.NoWeapon,
              traceSpeed: 1,
              traceSpriteWidthPixels: 0,
              traceStartOffsetPixels: 0);

    public static readonly WeaponFireTracePreset Pellets
        = new(traceTexturePath: "FX/WeaponTraces/TracePellets",
              hitSparksPreset: WeaponHitSparksPresets.Firearm,
              traceSpeed: 17,
              traceSpriteWidthPixels: 363,
              traceStartOffsetPixels: -10);

    public static readonly WeaponFireTracePreset Plasma
        = new(traceTexturePath: "FX/WeaponTraces/TracePlasma",
              hitSparksPreset: WeaponHitSparksPresets.Plasma,
              traceSpeed: 20,
              traceSpriteWidthPixels: 235,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -10,
              useScreenBlending: true,
              drawHitSparksAsLight: true);

    public static readonly WeaponFireTracePreset StunPlasma
        = new(traceTexturePath: "FX/WeaponTraces/TraceStunPlasma",
              hitSparksPreset: WeaponHitSparksPresets.Plasma,
              traceSpeed: 15,
              traceSpriteWidthPixels: 162,
              traceStartScaleSpeedExponent: 0.5,
              traceStartOffsetPixels: -13,
              useScreenBlending: false,
              drawHitSparksAsLight: true);
  }
}