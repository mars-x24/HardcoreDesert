namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeFuelCellPragmiumFromHeart : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectChemicalLab>();

      duration = CraftingDuration.Long;

      inputItems.Add<ItemFuelCellEmpty>(count: 1);
      inputItems.Add<ItemEnragedPragmiumHeart>(count: 1);
      inputItems.Add<ItemOreLithium>(count: 10);
      inputItems.Add<ItemComponentsIndustrialChemicals>(count: 5);

      outputItems.Add<ItemFuelCellPragmium>(count: 1);
      outputItems.Add<ItemPragmiumHeart>(count: 1);

      this.Icon = ClientItemIconHelper.CreateComposedIcon(
          name: this.Id + "Icon",
          primaryIcon: GetItem<ItemFuelCellPragmium>().Icon,
          secondaryIcon: GetItem<ItemEnragedPragmiumHeart>().Icon);
    }
  }
}