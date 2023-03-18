using AtomicTorch.CBND.CoreMod.Items.Devices;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeHuntersToolsSteel : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Short;

      inputItems.Add<ItemIngotSteel>(count: 3);
      inputItems.Add<ItemLeather>(count: 3);
      inputItems.Add<ItemTarpaulin>(count: 1);

      outputItems.Add<ItemHuntersToolsSteel>();
    }
  }
}