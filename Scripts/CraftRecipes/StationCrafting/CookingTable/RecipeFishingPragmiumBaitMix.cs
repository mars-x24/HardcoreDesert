using AtomicTorch.CBND.CoreMod.Items.Fishing;
using AtomicTorch.CBND.CoreMod.Items.Food;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeFishingPragmiumBaitMix : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan craftDuration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectCookingTable>();

      craftDuration = CraftingDuration.Medium;

      inputItems.Add<ItemDough>(count: 3);
      inputItems.Add<ItemMeatRaw>(count: 1);
      inputItems.Add<ItemInsectMeatRaw>(count: 2);
      inputItems.Add<ItemOrePragmium>(count: 1);

      outputItems.Add<ItemFishingPragmiumBaitMix>(count: 5);
    }
  }
}