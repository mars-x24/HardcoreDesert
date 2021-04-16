namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeBagFreezer : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Long;

      inputItems.Add<ItemPlastic>(count: 25);
      inputItems.Add<ItemWire>(count: 25);
      inputItems.Add<ItemComponentsElectronic>(count: 5);
      inputItems.Add<ItemBatteryHeavyDuty>(count: 2);

      outputItems.Add<ItemBagFreezer>();
    }
  }
}