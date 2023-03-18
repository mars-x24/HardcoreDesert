﻿using AtomicTorch.CBND.CoreMod.Items.Medical;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeHerbalMedicine : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectMedicalLab>();

      duration = CraftingDuration.Short;

      inputItems.Add<ItemRemedyHerbal>(count: 1);
      inputItems.Add<ItemStrengthBoostSmall>(count: 1);

      outputItems.Add<ItemMedicineHerbal>(count: 1);
    }
  }
}