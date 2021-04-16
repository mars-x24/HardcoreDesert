namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeVehicleRemoteControl : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemPlastic>(count: 40);
      inputItems.Add<ItemWire>(count: 100);
      inputItems.Add<ItemComponentsElectronic>(count: 20);
      inputItems.Add<ItemGoldNugget>(count: 5);

      outputItems.Add<ItemVehicleRemoteControl>();
    }
  }
}