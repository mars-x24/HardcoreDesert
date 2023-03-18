using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeTeleportAlien3 : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemEnrichedIngotLithium>(count: 1);
      inputItems.Add<ItemEnrichedIngotSteel>(count: 1);
      inputItems.Add<ItemEnrichedPlastic>(count: 2);

      outputItems.Add<ItemTeleportAlien3>();
    }
  }
}