namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeEnrichedPlastic : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectChemicalLab>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemPlastic>(count: 100);
      inputItems.Add<ItemKeinite>(count: 1);
      inputItems.Add<ItemCoal>(count: 5);
      inputItems.Add<ItemCanisterMineralOil>(count: 5);

      outputItems.Add<ItemEnrichedPlastic>(count: 5);
    }
  }
}