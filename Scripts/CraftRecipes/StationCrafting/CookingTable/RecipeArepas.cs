namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeArepas : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemCornBread>(count: 1);
            inputItems.Add<ItemInsectMeatFried>(count: 1);
            inputItems.Add<ItemSaladVegetable>(count: 1);

            outputItems.Add<ItemArepas>(count: 2);
        }
    }
}