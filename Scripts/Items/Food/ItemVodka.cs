﻿using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using System;

namespace AtomicTorch.CBND.CoreMod.Items.Food
{
  public class ItemVodka : ProtoItemFood
  {
    public override string Description =>
        "The fiery liquid of gods. Abuse leads to consequences, so don't drink too much. It also dehydrates you. And no, it doesn't help against radiation.";

    // Yes! Vodka is high in calories in real life, in fact 40% vodka is 75 kcal per 100 ml!
    public override float FoodRestore => 5;

    public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

    public override string ItemUseCaption => ItemUseCaptions.Drink;

    public override string Name => "Vodka";

    public override ushort OrganicValue => 0;

    public override float WaterRestore => -5; // dehydrates

    protected override void PrepareEffects(EffectActionsList effects)
    {
      // 4.5 minutes (so 2 vodka bottles will be almost 10 minutes, so after the third bottle you will be vomiting)
      effects
          .WillAddEffect<StatusEffectDrunk>(intensity: 0.45)
          .WillAddEffect<StatusEffectProtectionCold>(intensity: 0.40); //add cold protection
    }

    protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
    {
      return ItemsSoundPresets.ItemFoodDrinkAlcohol;
    }
  }
}