namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeEnrichedIngotLithium : Recipe.RecipeForManufacturing
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

      inputItems.Add<ItemIngotLithium>(count: 100);
      inputItems.Add<ItemKeinite>(count: 1);
      inputItems.Add<ItemFluxPowder>(count: 20);

      outputItems.Add<ItemEnrichedIngotLithium>(count: 5);
    }
  }
}