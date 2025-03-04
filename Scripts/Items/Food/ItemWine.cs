﻿using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using System;

namespace AtomicTorch.CBND.CoreMod.Items.Food
{
  public class ItemWine : ProtoItemFood
  {
    public override string Description =>
        "Majestic nectar made from the finest grapes (or the equivalent thereof). Exquisite taste and aroma.";

    public override float FoodRestore => 3; // Yes, wine has calories

    public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

    public override string ItemUseCaption => ItemUseCaptions.Drink;

    public override string Name => "Wine";

    public override ushort OrganicValue => 0;

    public override float WaterRestore => 3; // doesn't hydrate much, because of alcohol

    protected override void PrepareEffects(EffectActionsList effects)
    {
      effects
          .WillAddEffect<StatusEffectDrunk>(intensity: 0.25)
          .WillAddEffect<StatusEffectProtectionCold>(intensity: 0.10); //add cold protection
    }

    protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
    {
      return ItemsSoundPresets.ItemFoodDrinkAlcohol;
    }
  }
}