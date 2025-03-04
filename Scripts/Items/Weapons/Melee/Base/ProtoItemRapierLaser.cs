﻿namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
  using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.Base;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System.Windows.Media;

  public abstract class ProtoItemRapierLaser : ProtoItemWeaponMeleeEnergy, IProtoPiercingWeapon //MOD
  {
    public override double DamageApplyDelay => 0.1;

    public override string Description => "Incredibly powerful short-range weapon. Requires energy to operate.";

    public override uint DurabilityMax => 750;

    public override double EnergyUsePerHit => 25;

    public override double EnergyUsePerShot => 3;

    public override double FireAnimationDuration => 0.6;

    public override ITextureResource GroundIcon
        => new TextureResource("Items/Weapons/Melee/ItemRapierLaser/GroundIcon");

    public override double GroundIconScale => 1.2;

    public override double SpecialEffectProbability => 0.15; // 15%

    protected abstract Color LightColor { get; }

    protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

    protected override BaseClientComponentLightSource ClientCreateLightSource(
        IItem item,
        ICharacter character,
        IClientSceneObject sceneObject)
    {
      var lightSource = base.ClientCreateLightSource(item, character, sceneObject);
      // restore circle shape by using x2 height
      lightSource.RenderingSize = new Size2F(lightSource.RenderingSize.X, lightSource.RenderingSize.Y * 2);
      return lightSource;
    }

    protected override void PrepareProtoWeaponEnergy(
        ref DamageDescription overrideDamageDescription,
        ItemLightConfig lightConfig)
    {
      //MOD

      overrideDamageDescription = new DamageDescription(
          damageValue: 50,
          armorPiercingCoef: 0.7,
          finalDamageMultiplier: 1.2,
          rangeMax: 1.2,
          damageDistribution: new DamageDistribution(DamageType.Heat, 1));

      lightConfig.IsLightEnabled = true;
      lightConfig.Color = this.LightColor;
      lightConfig.ScreenOffset = (-20, -140);
      lightConfig.Size = 4;
      // we want to make it lighting objects around much better even though the rendering size is small
      lightConfig.LogicalSize = 16;
    }

    protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
    {
      return MaterialHitsSoundPresets.MeleeEnergy;
    }

    protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
    {
      return ItemsSoundPresets.ItemWeaponMeleeEnergyLaserRapier;
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return WeaponsSoundPresets.WeaponMeleeEnergyLaserRapier;
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      ServerWeaponSpecialEffectsHelper.OnLaserRapierHit(damagedCharacter, damage);
    }
  }
}