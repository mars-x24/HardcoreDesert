namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeEnrichedVialBiomaterial : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectChemicalLab>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemVialBiomaterial>(count: 50);
      inputItems.Add<ItemAlienBrain>(count: 10);
      inputItems.Add<ItemKeinite>(count: 1);

      outputItems.Add<ItemEnrichedVialBiomaterial>(count: 1);
    }
  }
}