namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeTeleportAlien1 : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemEnrichedIngotLithium>(count: 4);
      inputItems.Add<ItemEnrichedIngotSteel>(count: 2);
      inputItems.Add<ItemEnrichedPlastic>(count: 10);

      outputItems.Add<ItemTeleportAlien1>();
    }
  }
}