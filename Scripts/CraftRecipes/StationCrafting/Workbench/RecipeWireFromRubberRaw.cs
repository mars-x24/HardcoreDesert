using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.Items.Food;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using System;

namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  public class RecipeWireFromRubberRaw : Recipe.RecipeForStationCrafting
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectWorkbench>();

      duration = CraftingDuration.VeryShort;

      inputItems.Add<ItemIngotCopper>(count: 3);
      inputItems.Add<ItemRubberRaw>(count: 10);
      inputItems.Add<ItemBottleWater>(count: 1);
      inputItems.Add<ItemSulfurPowder>(count: 10);

      outputItems.Add<ItemWire>(count: 10);

      this.Icon = ClientItemIconHelper.CreateComposedIcon(
          name: this.Id + "Icon",
          primaryIcon: GetItem<ItemWire>().Icon,
          secondaryIcon: GetItem<ItemRubberRaw>().Icon);
    }
  }
}