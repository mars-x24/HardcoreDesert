namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
  using System.Collections.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Ammo;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Weapons;

  public class ItemHeavyRifleShort : ProtoItemWeaponRanged
  {
    public override ushort AmmoCapacity => 7;

    public override double AmmoReloadDuration => 3;

    public override double CharacterAnimationAimingRecoilDuration => 0.66;

    public override double CharacterAnimationAimingRecoilPower => 1.5;

    public override string Description => "Semi-automatic rifle with extreme muzzle energy.";

    public override uint DurabilityMax => 360;

    public override double FireInterval => 1.3;

    public override double DamageMultiplier => 1.3;

    public override double RangeMultiplier => 1.1;

    public override string Name => "Steppen Hunter";

    public override double SpecialEffectProbability => 0.30;

    protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

    protected override WeaponFirePatternPreset PrepareFirePatternPreset()
    {
      return new(
          initialSequence: new[] { 0.0, 1.0, 1.0 },
          cycledSequence: new[] { 1.5 });
    }

    protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
    {
      description.Set(MuzzleFlashPresets.ModernRifle)
                 .Set(textureScale: 1.8)
                 .Set(textureScreenOffset: (40, 8));
    }

    protected override void PrepareProtoWeaponRanged(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber50>();
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return WeaponsSoundPresets.WeaponRangedMagnum;
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
    }
  }
}