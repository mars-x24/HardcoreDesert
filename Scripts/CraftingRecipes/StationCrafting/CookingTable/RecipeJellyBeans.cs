namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
	using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeJellyBeans : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan craftDuration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            craftDuration = CraftingDuration.Medium;

            inputItems.Add<ItemBerriesJelly>(count:5);
			inputItems.Add<ItemSugar>(count:20);
			inputItems.Add<ItemButter>(count:1);
			inputItems.Add<ItemCornFlour>(count:1);
			inputItems.Add<ItemHerbPurple>(count:1);
			inputItems.Add<ItemHerbRed>(count:1);
			inputItems.Add<ItemHerbBlue>(count:1);
			

            outputItems.Add<ItemJellyBeans>(count: 10);
        }
    }
}