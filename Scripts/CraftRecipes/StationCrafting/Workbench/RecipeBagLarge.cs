namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeBagLarge : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Medium;

      inputItems.Add<ItemThread>(count: 20);
      inputItems.Add<ItemLeather>(count: 10);
      inputItems.Add<ItemRope>(count: 1);
      inputItems.Add<ItemGlue>(count: 3);

      outputItems.Add<ItemBagLarge>();
    }
  }
}