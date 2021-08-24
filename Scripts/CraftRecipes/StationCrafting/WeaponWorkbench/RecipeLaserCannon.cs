namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeLaserCannon : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemIngotCopper>(count: 120);
            inputItems.Add<ItemPlastic>(count: 40);
            inputItems.Add<ItemComponentsOptical>(count: 20);
            inputItems.Add<ItemComponentsHighTech>(count: 10);
            inputItems.Add<ItemPowerCell>(count: 3);
            inputItems.Add<ItemOrePragmium>(count: 20);

            outputItems.Add<ItemLaserCannon>();
        }
    }
}