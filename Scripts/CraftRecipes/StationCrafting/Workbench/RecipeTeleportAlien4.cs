using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeTeleportAlien4 : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemEnrichedIngotLithium>(count: 8);
      inputItems.Add<ItemEnrichedIngotSteel>(count: 4);
      inputItems.Add<ItemEnrichedPlastic>(count: 20);

      outputItems.Add<ItemTeleportAlien4>();
    }
  }
}