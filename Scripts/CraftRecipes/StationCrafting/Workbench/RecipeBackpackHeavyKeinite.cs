namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeBackpackHeavyKeinite : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectArmorerWorkbench>();

      duration = CraftingDuration.Long;

      inputItems.Add<ItemKeinite>(count: 20);
      inputItems.Add<ItemIngotSteel>(count: 50);
      inputItems.Add<ItemAramidFiber>(count: 20);
      inputItems.Add<ItemComponentsHighTech>(count: 2);
      inputItems.Add<ItemBallisticPlate>(count: 5);
      inputItems.Add<ItemTarpaulin>(count: 20);
      inputItems.Add<ItemGlue>(count: 20);
      inputItems.Add<ItemFur>(count: 50);

      outputItems.Add<ItemBackpackHeavyKeinite>();
    }
  }
}