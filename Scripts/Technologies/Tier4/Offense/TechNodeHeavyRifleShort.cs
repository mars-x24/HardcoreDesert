﻿using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
  public class TechNodeHeavyRifleShort : TechNode<TechGroupOffenseT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeHeavyRifleShort>();

      config.SetRequiredNode<TechNodeSteppenHawk>();
    }
  }
}