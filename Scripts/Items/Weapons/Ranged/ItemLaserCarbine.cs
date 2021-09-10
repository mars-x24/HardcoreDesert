namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
  using System.Collections.Generic;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
  using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;

  public class ItemLaserCarbine : ProtoItemWeaponRangedEnergy
  {
    private static readonly TextureResource TextureResourceBeam
        = new("FX/WeaponTraces/TraceBeamLaser.png");

    public override double CharacterAnimationAimingRecoilDuration => 0.1;

    public override double CharacterAnimationAimingRecoilPower => 0.15;

    public override string Description =>
        "This laser semi-automatic carbine by Vanguard Technologies allows a double laser shot by light splitting in its pragmium crystal diffuser, though excesive vibration from its high rate of fire can produce imprevisible deviation of light which translates into extreme recoil patterns.";

    public override uint DurabilityMax => 800;

    public override double EnergyUsePerShot => 50;

    public override double FireInterval => 0.04;

    public override string Name => "Vanguard Laser carbine";

    public override float ShotVolumeMultiplier => 1.50f;

    public override double SpecialEffectProbability => 0.4;

    public override double FirePatternCooldownDuration => this.FireInterval + 0.10;

    public override double ReadyDelayDuration => 1.0;

    public override bool IsSemiAutomatic => true;

    protected override WeaponFireScatterPreset PrepareFireScatterPreset()
    {
      return new(
          new[] { -0.25, 0.25 });
    }

    protected override WeaponFirePatternPreset PrepareFirePatternPreset()
    {
      return new(
          initialSequence: new[] { 0.0, 0.5 },
          cycledSequence: new[] { 5.0, -8.0, 10, -12, 16, -18 });
    }

    public override void ClientOnWeaponHitOrTrace(
        ICharacter firingCharacter,
        Vector2D worldPositionSource,
        IProtoItemWeapon protoWeapon,
        IProtoItemAmmo protoAmmo,
        IProtoCharacter protoCharacter,
        in Vector2Ushort fallbackCharacterPosition,
        IReadOnlyList<WeaponHitData> hitObjects,
        in Vector2D endPosition,
        bool endsWithHit)
    {
      ComponentWeaponEnergyBeam.Create(
          sourcePosition: worldPositionSource,
          targetPosition: endPosition,
          traceStartWorldOffset: 0.25,
          texture: TextureResourceBeam,
          beamWidth: 0.005,
          originOffset: Vector2D.Zero,
          duration: 0.1,
          endsWithHit,
          fadeInDistance: 0.2,
          fadeOutDistanceHit: 0,
          fadeOutDistanceNoHit: 0.8,
          blendMode: BlendMode.AlphaBlendPremultiplied);
    }

    public override void SharedOnHit(
        WeaponFinalCache weaponCache,
        IWorldObject damagedObject,
        double damage,
        WeaponHitData hitData,
        out bool isDamageStop)
    {
      base.SharedOnHit(weaponCache, damagedObject, damage, hitData, out isDamageStop);

      if (damage < 1)
      {
        return;
      }

      if (IsServer
          && damagedObject is ICharacter damagedCharacter
          && RandomHelper.RollWithProbability(0.2))
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(
            // add 0.2 seconds of dazed effect
            intensity: 0.2 / StatusEffectDazed.MaxDuration);
      }
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);
      ComponentWeaponEnergyBeam.PreloadAssets();
      Client.Rendering.PreloadTextureAsync(TextureResourceBeam);
    }

    protected override WeaponFireTracePreset PrepareFireTracePreset()
    {
      // see ClientOnWeaponHitOrTrace for the custom laser ray implementation
      return WeaponFireTracePresets.LaserBlue;
    }

    protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
    {
      description.Set(MuzzleFlashPresets.EnergyLaserWeaponBlue)
                 .Set(textureScreenOffset: (28, 9));
    }

    protected override void PrepareProtoWeaponRangedEnergy(
        ref DamageDescription damageDescription)
    {
      damageDescription = new DamageDescription(
          damageValue: 11,
          armorPiercingCoef: 0.3,
          finalDamageMultiplier: 1.2,
          rangeMax: 11,
          damageDistribution: new DamageDistribution(DamageType.Heat, 1));
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return WeaponsSoundPresets.WeaponRangedLaserCarbine;
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      ServerWeaponSpecialEffectsHelper.OnLaserHit(damagedCharacter, damage);
      // also, see SharedOnHit as it adds Dazed
    }
  }
}