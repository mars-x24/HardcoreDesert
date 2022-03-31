namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
  using AtomicTorch.CBND.CoreMod.Systems;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class RecipeKeiniteEnraged : Recipe.RecipeForManufacturing
  {
    protected override void SetupRecipe(
        StationsList stations,
        out TimeSpan duration,
        InputItems inputItems,
        OutputItems outputItems)
    {
      stations.Add<ObjectFurnaceElectric>();

      duration = CraftingDuration.Medium;

      inputItems.Add<ItemIngotLithium>(count: 20);
      inputItems.Add<ItemMeatRawEnraged>(count: 5);
      inputItems.Add<ItemCanisterGasoline>(count: 20);

      outputItems.Add<ItemKeinite>(count: 1);
      outputItems.Add<ItemAsh>(count: 5);
      outputItems.Add<ItemCanisterEmpty>(count: 20);
    }

    protected override bool CanBeCrafted(IStaticWorldObject objectManufacturer, CraftingQueue craftingQueue, ushort countToCraft)
    {
      if (!base.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft))
        return false;

      //No Shield for this recipe
      if (craftingQueue.QueueItems.Count > 0)
      {
        var craftingQueueItem = craftingQueue.QueueItems[0];

        if (craftingQueueItem.GameObject is IStaticWorldObject staticWorldObject
          && LandClaimShieldProtectionSystem.SharedIsUnderShieldProtection(staticWorldObject))
        {
          // pause crafting while protected by shield
          return false;
        }
      }

      return true;
    }

    public static double MinDistanceBetweenSpawnedObjects => 2;

    public static byte CircleRadius = 35;

    public override void ServerOnManufacturingCompleted(IStaticWorldObject objectManufacturer, CraftingQueue craftingQueue)
    {
      base.ServerOnManufacturingCompleted(objectManufacturer, craftingQueue);

      var enragedMobs = ServerMobSpawnHelper.GetCloseEnragedMobs(objectManufacturer.TilePosition, CircleRadius);

      //aggro close enraged mobs 
      ServerMobSpawnHelper.ChangeEnragedMobsGoal(objectManufacturer, enragedMobs, true);

      //spawn more enraged mobs
      ServerMobSpawnHelper.ServerTrySpawnMobsEnraged(objectManufacturer, MinDistanceBetweenSpawnedObjects, CircleRadius, 3);
    }

    protected override void ValidateRecipe()
    {
      base.ValidateRecipe();
    }
  }
}