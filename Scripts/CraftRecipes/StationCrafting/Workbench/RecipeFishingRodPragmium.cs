﻿using AtomicTorch.CBND.CoreMod.Items.Fishing;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeFishingRodPragmium : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Medium;

      inputItems.Add<ItemThread>(count: 10);
      inputItems.Add<ItemOrePragmium>(count: 10);
      inputItems.Add<ItemPlastic>(count: 5);

      outputItems.Add<ItemFishingRodPragmium>();
    }
  }
}