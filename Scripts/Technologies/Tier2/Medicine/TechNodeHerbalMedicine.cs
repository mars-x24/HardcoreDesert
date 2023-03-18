﻿using AtomicTorch.CBND.CoreMod.CraftRecipes;
using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
  public class TechNodeHerbalMedicine : TechNode<TechGroupMedicineT2>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeHerbalMedicine>();

      config.SetRequiredNode<TechNodeStrengthBoostSmall>();
    }
  }
}