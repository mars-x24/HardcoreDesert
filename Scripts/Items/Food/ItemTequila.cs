﻿using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using System;

namespace AtomicTorch.CBND.CoreMod.Items.Food
{
  public class ItemTequila : ProtoItemFood
  {
    public override string Description =>
        "Beautiful liquor distilled from plants found in the desert.";

    public override float FoodRestore => 5; // Yes, alcohol has calories

    public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

    public override string ItemUseCaption => ItemUseCaptions.Drink;

    public override string Name => "Tequila";

    public override ushort OrganicValue => 0;

    public override float WaterRestore => -5; // dehydrates

    protected override void PrepareEffects(EffectActionsList effects)
    {
      effects
          .WillAddEffect<StatusEffectDrunk>(intensity: 0.40)
          .WillAddEffect<StatusEffectProtectionCold>(intensity: 0.15); //add cold protection
    }

    protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
    {
      return ItemsSoundPresets.ItemFoodDrinkAlcohol;
    }
  }
}