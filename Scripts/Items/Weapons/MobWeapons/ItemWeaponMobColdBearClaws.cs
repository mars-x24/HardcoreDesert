﻿using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
using AtomicTorch.CBND.CoreMod.Items.Ammo;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Weapons;
using AtomicTorch.GameEngine.Common.Helpers;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
  public class ItemWeaponMobColdBearClaws : ProtoItemMobWeaponMelee
  {
    public override double DamageApplyDelay => 0.15;

    public override double FireAnimationDuration => 0.9;

    public override double FireInterval => 1.5;

    protected override void PrepareProtoWeapon(
        out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
        ref DamageDescription overrideDamageDescription)
    {
      // no ammo used
      compatibleAmmoProtos = null;

      overrideDamageDescription = new DamageDescription(
          damageValue: 60,
          armorPiercingCoef: 0.3,
          finalDamageMultiplier: 1,
          rangeMax: 1.5,
          damageDistribution: new DamageDistribution()
                                    .Set(DamageType.Impact, 0.7)
                                    .Set(DamageType.Cold, 0.3));
    }

    protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
    {
      return MaterialHitsSoundPresets.MeleeNoWeapon;
    }

    protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
    {
      if (RandomHelper.RollWithProbability(0.40))
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(intensity: 0.1);
      }

      if (RandomHelper.RollWithProbability(0.25))
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.2);
        damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.1);
      }

      if (RandomHelper.RollWithProbability(0.1))
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectBrokenLeg>(intensity: 1);
        damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.2);
      }

      if (RandomHelper.RollWithProbability(0.1))
      {
        damagedCharacter.ServerAddStatusEffect<StatusEffectLaceration>(intensity: 0.4);
      }
    }
  }
}