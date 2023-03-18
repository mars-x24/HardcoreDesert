using AtomicTorch.CBND.CoreMod.Items.Food;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeCornBread : Recipe.RecipeForManufacturing
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectStove>();
      stations.Add<ObjectStoveElectric>();

      duration = CraftingDuration.Medium;

      inputItems.Add<ItemCornflourDough>(count: 3);

      outputItems.Add<ItemCornBread>(count: 1);
    }
  }
}