using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeEnrichedIngotSteel : Recipe.RecipeForManufacturing
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectFurnace>()
              .Add<ObjectFurnaceElectric>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemIngotSteel>(count: 20);
      inputItems.Add<ItemKeinite>(count: 1);
      inputItems.Add<ItemCharcoal>(count: 5);
      inputItems.Add<ItemFluxPowder>(count: 5);

      outputItems.Add<ItemEnrichedIngotSteel>();
    }
  }
}