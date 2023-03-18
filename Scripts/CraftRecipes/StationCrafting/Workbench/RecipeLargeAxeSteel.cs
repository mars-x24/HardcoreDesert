﻿using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeLargeAxeSteel : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Medium;

      inputItems.Add<ItemPlanks>(count: 20);
      inputItems.Add<ItemIngotSteel>(count: 6);

      outputItems.Add<ItemLargeAxeSteel>();
    }
  }
}