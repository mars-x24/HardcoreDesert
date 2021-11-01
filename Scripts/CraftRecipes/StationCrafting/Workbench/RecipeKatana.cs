namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeKatana : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Long;

      inputItems.Add<ItemLeather>(count: 1);
      inputItems.Add<ItemThread>(count: 5);
      inputItems.Add<ItemIngotSteel>(count: 15);
      inputItems.Add<ItemGoldNugget>(count: 8);

      outputItems.Add<ItemKatana>();
    }
  }
}