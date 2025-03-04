﻿using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
using AtomicTorch.CBND.CoreMod.Skills;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Physics;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  public abstract class ProtoItemMobNPCWeaponRanged : ProtoItemWeaponRanged
  {
    public override ushort AmmoCapacity => 10;

    public override double AmmoReloadDuration => 5;

    public override bool CanDamageStructures => true;

    public override CollisionGroup CollisionGroup => CollisionGroups.HitboxRanged;

    public override double DamageApplyDelay => 0;

    public override string Description => null;

    public override uint DurabilityMax => 0;

    public override bool IsLoopedAttackAnimation => false;

    public override string Name => this.ShortId;

    public override double ReadyDelayDuration => 0;

    public override (float min, float max) SoundPresetWeaponDistance
        => (SoundConstants.AudioListenerMinDistanceRangedShot,
            SoundConstants.AudioListenerMaxDistanceRangedShotFirearms);

    public override double SpecialEffectProbability =>
        1; // Must always be 1 for all animal weapons. Individual effects will be rolled in the effect function.

    protected override ProtoSkillWeapons WeaponSkill => null;

    protected override TextureResource WeaponTextureResource => null;

    public override void ClientSetupSkeleton(
        IItem item,
        ICharacter character,
        ProtoCharacterSkeleton protoCharacterSkeleton,
        IComponentSkeleton skeletonRenderer,
        List<IClientComponent> skeletonComponents,
        bool isPreview = false)
    {
      // do nothing
    }

    public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
    {
      return character.ProtoCharacter is IProtoCharacterMob;
    }

    protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
    {
      description.TextureAtlas = null;
      description.LightPower = 0;
    }

    protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
    {
      return MaterialHitsSoundPresets.Ranged;
    }

    protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
    {
      return WeaponsSoundPresets.WeaponRanged;
    }

    public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
    {
      return true;
    }

    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      return true;
    }
  }
}