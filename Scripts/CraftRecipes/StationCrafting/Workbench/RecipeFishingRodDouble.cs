namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Fishing;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeFishingRodDouble : Recipe.RecipeForStationCrafting
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
      inputItems.Add<ItemThread>(count: 10);
      inputItems.Add<ItemIngotIron>(count: 5);
      inputItems.Add<ItemPlastic>(count: 5);

      outputItems.Add<ItemFishingRodDouble>();
    }
  }
}