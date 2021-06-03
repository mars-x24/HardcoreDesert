namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using System;

  public class RecipeVehicleCustomCannonEnergy : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWeaponWorkbench>();

      duration = CraftingDuration.VeryLong;

      inputItems.Add<ItemVehicleCannonEnergy>(count: 1);
      inputItems.Add<ItemCanisterMineralOil>(count: 1);
      inputItems.Add<ItemFlowerYellow>(count: 5);
      inputItems.Add<ItemComponentsMechanical>(count: 1);

      outputItems.Add<ItemVehicleCustomCannonEnergy>();
    }
  }
}