namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeJelly : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();
            stations.Add<ObjectStoveElectric>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemBerriesJelly>(count: 3);
            inputItems.Add<ItemSugar>(count: 3);
			inputItems.Add<ItemBones>(count: 1);
			inputItems.Add<ItemBottleWater>(count: 1);

            outputItems.Add<ItemJelly>();
			outputItems.Add<ItemBottleEmpty>();
		}
            /*this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemJelly>().Icon,
                secondaryIcon: GetItem<ItemBerriesJelly>().Icon);*/
        
    }
}