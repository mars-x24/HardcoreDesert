namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeHipbag : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>().Add<ObjectArmorerWorkbench>();

      duration = CraftingDuration.Medium;

      inputItems.Add<ItemThread>(count: 20);
      inputItems.Add<ItemRope>(count: 2);
      inputItems.Add<ItemGlue>(count: 3);
      inputItems.Add<ItemBones>(count: 5);

      outputItems.Add<ItemHipbag>();
    }
  }
}