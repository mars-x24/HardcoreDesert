﻿using AtomicTorch.CBND.CoreMod.Items.Food;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeCornFlour : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan craftDuration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectCookingTable>();

      craftDuration = CraftingDuration.VeryShort;

      inputItems.Add<ItemCorn>(count: 3);

      outputItems.Add<ItemCornFlour>(count: 1);
    }
  }
}