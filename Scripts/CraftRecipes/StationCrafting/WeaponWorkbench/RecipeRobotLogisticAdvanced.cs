using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeRobotLogisticAdvanced : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.Short;

      inputItems.Add<ItemIngotSteel>(count: 10);
      inputItems.Add<ItemComponentsElectronic>(count: 10);
      inputItems.Add<ItemComponentsOptical>(count: 10);
      inputItems.Add<ItemPowerCell>(count: 10);

      outputItems.Add<ItemRobotLogisticAdvanced>();
    }
  }
}