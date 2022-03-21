using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
using AtomicTorch.CBND.CoreMod.Systems.Crafting;
using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
using AtomicTorch.CBND.GameApi.Data;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HardcoreDesert.Scripts.Systems.Robot
{
  public class RobotItemHelper
  {
    public RobotOwner RobotOwner
    {
      get { return this.robotOwner; }
    }
    private RobotOwner robotOwner;

    public IStaticWorldObject Target
    {
      get { return this.target; }
    }
    private IStaticWorldObject target;

    public bool TargetIsActive
    {
      get { return this.targetIsActive; }
    }
    private bool targetIsActive;

    public Dictionary<IItem, ushort> TargetItems
    {
      get { return this.targetItems; }
    }
    private Dictionary<IItem, ushort> targetItems;

    public Dictionary<IProtoItem, ushort> InputItems
    {
      get { return this.inputItems; }
    }
    private Dictionary<IProtoItem, ushort> inputItems;

    public Dictionary<IProtoItem, ushort> FuelItems
    {
      get { return this.fuelItems; }
    }
    private Dictionary<IProtoItem, ushort> fuelItems;

    public bool HasItems
    {
      get { return this.inputItems.Count + this.fuelItems.Count + this.targetItems.Count != 0; }
    }

    private List<IStaticWorldObject> outputManufacturer, inputManufacturer;

    private IDynamicWorldObject robotObject;
    private IProtoItemRobot robotProtoItem;
    private IItemsContainer parentContainer;
    private IProtoEntity targetItemProto;

    private IStaticWorldObject currentTarget = null;
    private bool currentTargetIsActive = false;
    private bool currentTargetIsActiveUsed = true;
    private StructurePrivateState currentPrivateState = null;
    private IItemsContainer currentInputContainer = null;
    private IItemsContainer currentFuelContainer = null;

    private bool inputAllowed = false;
    private bool outputAllowed = false;
    private bool fuelAllowed = false;

    private byte loadPercent = ItemRobotPrivateState.DEFAULT_STRUCTURE_LOAD_PERCENT;
    private bool loadInactiveOnly = false;

    private bool inputDone = false;
    private bool outputDone = false;

    public RobotItemHelper(RobotOwner robotOwner, List<IStaticWorldObject> outputManufacturer, List<IStaticWorldObject> inputManufacturer)
    {
      this.robotOwner = robotOwner;

      this.robotObject = robotOwner.RobotObject;

      var itemRobot = robotObject.GetPrivateState<RobotPrivateState>()?.AssociatedItem;
      this.robotProtoItem = itemRobot.ProtoGameObject as IProtoItemRobot;

      var state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      if (state is not null)
      {
        this.inputAllowed = state.RobotManufacturerInputEnabled;
        this.outputAllowed = state.RobotManufacturerOutputEnabled;
        this.fuelAllowed = state.RobotManufacturerFuelEnabled;

        this.loadPercent = state.StructureLoadPercent;
        this.loadInactiveOnly = state.LoadInactiveOnly;
      }

      this.parentContainer = robotOwner.RobotItem.Container;

      this.target = null;
      this.targetIsActive = false;

      this.targetItems = new Dictionary<IItem, ushort>();
      this.inputItems = new Dictionary<IProtoItem, ushort>();
      this.fuelItems = new Dictionary<IProtoItem, ushort>();

      this.targetItemProto = robotOwner.TargetItemProto;

      this.outputManufacturer = outputManufacturer;
      this.inputManufacturer = inputManufacturer;
    }

    public void FindAllItems()
    {
      this.FindIdleManufacturerItems();

      this.FindOutputItems();

      this.FindInputItems();
    }

    private void FindIdleManufacturerItems()
    {
      foreach (IStaticWorldObject m in this.inputManufacturer)
      {
        var state = m.GetPublicState<ObjectManufacturerPublicState>();
        if (state is null)
          continue;

        if (state.IsActive)
          continue;

        this.FindOutputItems(m);

        this.FindInputItems(m);
      }
    }

    private void FindInputItems()
    {
      foreach (IStaticWorldObject m in this.inputManufacturer)
      {
        this.FindInputItems(m);
      }
    }

    private void FindInputItems(IStaticWorldObject m)
    {
      if (this.inputDone)
        return;

      if (m.ProtoGameObject is ProtoObjectBarrel)
      {
        //separate cases
        return;
      }

      //electricity off
      if (m.ProtoGameObject is IProtoObjectElectricityConsumer o && m.ProtoGameObject is not IProtoObjectElectricityProducer)
      {
        //Looks like we can't really use IProtoObjectElectricityConsumer since all ObjectManufacturer uses this proto
        if (o.ElectricityConsumptionPerSecondWhenActive != 0)
        {
          if (o.GetPublicState(m).ElectricityConsumerState == ElectricityConsumerState.PowerOff)
            return;
        }
      }

      if (!this.SetCurrent(m))
        return;

      var recipes = this.GetCurrentRecipes();
      for (int i = 0; i < recipes.Count; i++)
      {
        this.SetCurrentContainer(i);

        this.FillFuelItem();

        this.RecipeRemoveUselessItems(recipes[i]);

        this.FindItemsWithRecipe(recipes[i]);
      }

      if (this.inputItems.Count + this.fuelItems.Count > 0)
        this.inputDone = true;
    }

    private void FillFuelItem()
    {
      if (!this.fuelAllowed)
        return;

      if (this.currentFuelContainer is null)
        return;

      if (this.currentFuelContainer.EmptySlotsCount != this.currentFuelContainer.SlotsCount)
        return;

      //find the best fuel
      var fuelItems = this.parentContainer.GetItemsOfProto<IProtoItemFuelSolid>().OrderBy(f => -((IProtoItemFuelSolid)f.ProtoGameObject).FuelAmount).ToList();
      if (fuelItems.Count == 0)
        return;

      this.AddTargetItem(fuelItems[0], true, fuelItems[0].Count, true);
    }

    private void FindOutputItems()
    {
      foreach (IStaticWorldObject m in this.outputManufacturer)
      {
        this.FindOutputItems(m);
      }
    }

    private void FindOutputItems(IStaticWorldObject m)
    {
      if (this.outputDone)
        return;

      if (!this.outputAllowed)
        return;

      if (this.parentContainer.EmptySlotsCount == 0)
        return;

      if (!this.SetCurrent(m))
        return;

      var itemOrdered = RobotTargetHelper.GetOutputContainersItems(m, false);

      foreach (var item in itemOrdered)
      {
        if (!RobotTargetHelper.ServerPickupAllowed(item, this.robotObject))
          continue;

        this.AddTargetItem(item, false, ushort.MaxValue);
      }

      if (this.targetItems.Count > 0)
        this.outputDone = true;
    }

    private List<Recipe> GetCurrentRecipes()
    {
      List<Recipe> list = new List<Recipe>();

      var manufacturerConfigs = this.GetManufacturerConfigs();
      foreach (var manufacturerConfig in manufacturerConfigs)
      {
        if (manufacturerConfig.IsAutoSelectRecipe && manufacturerConfig.Recipes.Length > 0)
          list.Add(manufacturerConfig.Recipes[0]);
      }

      if (this.currentPrivateState is ObjectManufacturerPrivateState privateStateManufacturer)
      {
        Recipe selectedRecipe = privateStateManufacturer.ManufacturingState.SelectedRecipe;
        if (selectedRecipe is not null)
        {
          if (list.Count > 0)
            list[0] = selectedRecipe;
          else
            list.Add(selectedRecipe);
        }
      }

      return list;
    }

    private List<ManufacturingConfig> GetManufacturerConfigs()
    {
      List<ManufacturingConfig> list = new List<ManufacturingConfig>();

      if (this.currentTarget.ProtoGameObject is ProtoObjectManufacturer protoManufacturer)
        list.Add(protoManufacturer.ManufacturingConfig);

      else if (this.currentTarget.ProtoGameObject is ProtoObjectExtractor protoExtractor)
        list.Add(protoExtractor.ManufacturingConfig);

      else if (this.currentTarget.ProtoGameObject is ProtoObjectOilRefinery protoOilRefinery)
      {
        list.Add(protoOilRefinery.ManufacturingConfig);
        list.Add(protoOilRefinery.ManufacturingConfigGasoline);
        list.Add(protoOilRefinery.ManufacturingConfigMineralOil);
      }

      else if (this.currentTarget.ProtoGameObject is ProtoObjectOilCrackingPlant protoOilCrackingPlant)
      {
        list.Add(protoOilCrackingPlant.ManufacturingConfig);
        list.Add(protoOilCrackingPlant.ManufacturingConfigGasoline);
      }

      else if (this.currentTarget.ProtoGameObject is ObjectGeneratorEngine protoObjectGeneratorEngine)
      {
        list.Add(protoObjectGeneratorEngine.ManufacturingConfig);
      }
      
      else if (this.currentTarget.ProtoGameObject is ProtoObjectSprinkler protoObjectSprinkler)
      {
        list.Add(protoObjectSprinkler.ManufacturingConfig);
      }

      return list;
    }

    private bool IsItemAllowed(IItem item)
    {
      return this.targetItemProto is null || (item.ProtoGameObject.GetType() == this.targetItemProto.GetType());
    }

    private bool DeliveryFull(bool isInput)
    {
      byte count = 0;

      if (isInput)
      {
        count += this.GetStackCountProto(this.inputItems);
        count += this.GetStackCountProto(this.fuelItems);
      }
      else
      {
        count += (byte)this.targetItems.Count;
      }

      return count >= this.robotProtoItem.ItemDeliveryCount;
    }

    private byte GetStackCountProto(Dictionary<IProtoItem, ushort> list)
    {
      byte count = 0;
      foreach (var protoItem in list.Keys)
      {
        count += (byte)Math.Ceiling((double)list[protoItem] / (double)protoItem.MaxItemsPerStack);
      }
      return count;
    }

    private bool IsStructureAllowed()
    {
      return this.target is null || (this.target == this.currentTarget);
    }

    private bool AddTargetItem(IItem item, bool isInput, ushort count, bool isFuel = false)
    {
      if (this.loadInactiveOnly && this.currentTargetIsActive && this.currentTargetIsActiveUsed)
        return false;

      if (this.DeliveryFull(isInput))
        return false;

      if (!this.IsItemAllowed(item))
        return false;

      if (!this.IsStructureAllowed())
        return false;

      if (isInput)
      {
        if (isFuel)
        {
          if (!this.fuelItems.Keys.Contains(item.ProtoItem))
            this.fuelItems.Add(item.ProtoItem, count);
          else
            this.fuelItems[item.ProtoItem] = (ushort)((ushort)this.fuelItems[item.ProtoItem] + count);
        }
        else
        {
          if (!this.inputItems.Keys.Contains(item.ProtoItem))
            this.inputItems.Add(item.ProtoItem, count);
          else
            this.inputItems[item.ProtoItem] = (ushort)((ushort)this.inputItems[item.ProtoItem] + count);
        }
      }
      else
      {
        if (this.parentContainer.EmptySlotsCount <= this.targetItems.Count)
          return false;

        if (!this.targetItems.Keys.Contains(item))
          this.targetItems.Add(item, count);
      }

      this.target = this.currentTarget;
      this.targetIsActive = this.currentTargetIsActive;

      return true;
    }


    private void RecipeRemoveUselessItems(Recipe recipe)
    {
      if (!this.inputAllowed)
        return;

      foreach (var item in this.currentInputContainer.Items)
      {
        if (!recipe.InputItems.Any(it => it.ProtoItem == item.ProtoItem))
        {
          if (!this.AddTargetItem(item, false, item.Count))
            continue;
        }
      }
    }

    private void FindItemsWithRecipe(Recipe recipe)
    {
      if (!this.inputAllowed)
        return;

      if (recipe is null)
        return;

      int recipeCount = this.GetRecipeCountWithLoadPercent(this.currentInputContainer, recipe);

      Dictionary<IProtoItem, ushort> recipeItemMoveCount = new Dictionary<IProtoItem, ushort>();

      //get item count
      foreach (var recipeItem in recipe.InputItems)
      {
        ushort currentCount = Convert.ToUInt16(this.currentInputContainer.CountItemsOfType(recipeItem.ProtoItem));
        int moveCount = recipeItem.Count * recipeCount;

        moveCount -= currentCount;

        if (moveCount == 0)
          continue;

        if (moveCount < 0)
        {
          var itemsToMove = this.currentInputContainer.GetItemsOfProto(recipeItem.ProtoItem).OrderBy(i => i.Count).ToList();

          if (itemsToMove.Count > 0)
            this.AddTargetItem(itemsToMove[0], false, count: Convert.ToUInt16(-moveCount));
          continue;
        }

        recipeItemMoveCount[recipeItem.ProtoItem] = Convert.ToUInt16(moveCount);
      }

      //match up items
      foreach (var protoItem in recipeItemMoveCount.Keys)
      {
        int moveCount = recipeItemMoveCount[protoItem];

        var items = this.parentContainer.GetItemsOfProto(protoItem);

        foreach (IItem item in items)
        {
          if (moveCount <= 0)
            break;

          if (item.Count <= moveCount)
          {
            if (this.AddTargetItem(item, true, item.Count))
              moveCount -= item.Count;
          }
          else
          {
            if (this.AddTargetItem(item, true, count: Convert.ToUInt16(moveCount)))
              moveCount = 0;
          }
        }
      }
    }

    private int GetRecipeCountWithLoadPercent(IItemsContainer itemContainer, Recipe recipe)
    {
      int count = this.GetRecipeCount(itemContainer, recipe);

      if (count <= 1)
        return count;

      int newCount = (int)Math.Round(count * this.loadPercent / 100.0, 0);
      if (newCount <= 1)
        return count;

      return newCount;
    }

    private int GetRecipeCount(IItemsContainer itemContainer, Recipe recipe)
    {
      if (recipe is null)
        return 0;

      int slotCount = itemContainer.SlotsCount;
      int slotFactor = (int)Math.Floor((double)slotCount / (double)recipe.InputItems.Length);
      int itemFactor = int.MaxValue;
      foreach (var recipeItem in recipe.InputItems)
        itemFactor = Math.Min(itemFactor, recipeItem.ProtoItem.MaxItemsPerStack / recipeItem.Count);
      if (itemFactor == int.MaxValue)
        itemFactor = 1;

      int factor = slotFactor * itemFactor;
      int slotsNeeded = this.GetRecipeSlotCount(factor, recipe);
      if (slotsNeeded == slotCount)
        return factor;

      int testFactor = (int)Math.Floor((double)slotCount / (double)slotsNeeded * (double)factor);
      slotsNeeded = this.GetRecipeSlotCount(testFactor, recipe);
      if (slotsNeeded <= slotCount)
        factor = testFactor;
      else
      {
        do
        {
          testFactor--;
          slotsNeeded = this.GetRecipeSlotCount(testFactor, recipe);
        }
        while (slotsNeeded > slotCount);

        factor = testFactor;
      }

      return factor;
    }

    private int GetRecipeSlotCount(int factor, Recipe recipe)
    {
      int count = 0;
      foreach (var recipeItem in recipe.InputItems)
        count += (int)Math.Ceiling((double)(recipeItem.Count * factor) / (double)recipeItem.ProtoItem.MaxItemsPerStack);
      return count;
    }

    private bool SetCurrent(IStaticWorldObject m)
    {
      if (this.currentTarget == m)
        return true;

      //if an item is found, lock the current structure
      if (this.inputItems.Count + this.targetItems.Count + this.fuelItems.Count > 0)
        return false;

      var state = m.GetPublicState<ObjectManufacturerPublicState>();
      if (state is not null)
      {
        if (this.loadInactiveOnly && state.IsActive)
          return false;
      }

      if (this.currentTarget != m)
      {
        this.inputItems.Clear();
        this.fuelItems.Clear();
        this.targetItems.Clear();
      }

      this.currentTarget = m;
      this.currentPrivateState = m.GetPrivateState<StructurePrivateState>();
      this.currentTargetIsActive = state is not null ? state.IsActive : true;
      this.currentTargetIsActiveUsed = state is not null;

      return true;
    }

    private void SetCurrentContainer(int containerNumber)
    {
      this.currentFuelContainer = null;

      if (this.currentPrivateState is ProtoObjectOilRefinery.PrivateState privateStateOilRefinery)
      {
        if (containerNumber == 2)
        {
          this.currentInputContainer = privateStateOilRefinery.ManufacturingStateMineralOil.ContainerInput;
          //this.currentOutputContainer = privateStateOilRefinery.ManufacturingStateMineralOil.ContainerOutput;
        }
        else if (containerNumber == 1)
        {
          this.currentInputContainer = privateStateOilRefinery.ManufacturingStateGasoline.ContainerInput;
          //this.currentOutputContainer = privateStateOilRefinery.ManufacturingStateGasoline.ContainerOutput;
        }
        else
        {
          this.currentInputContainer = privateStateOilRefinery.ManufacturingState.ContainerInput;
          //this.currentOutputContainer = privateStateOilRefinery.ManufacturingState.ContainerOutput;
        }
      }

      else if (this.currentPrivateState is ProtoObjectOilCrackingPlant.PrivateState privateStateCrackingPlant)
      {
        if (containerNumber == 1)
        {
          this.currentInputContainer = privateStateCrackingPlant.ManufacturingStateGasoline.ContainerInput;
          //this.currentOutputContainer = privateStateCrackingPlant.ManufacturingStateGasoline.ContainerOutput;
        }
        else
        {
          this.currentInputContainer = privateStateCrackingPlant.ManufacturingState.ContainerInput;
          //this.currentOutputContainer = privateStateCrackingPlant.ManufacturingState.ContainerOutput;
        }
      }
      else if (this.currentPrivateState is ProtoObjectSprinkler.PrivateState privateStateSprinkler)
      {
        this.currentInputContainer = privateStateSprinkler.ManufacturingState.ContainerInput;
      }
      else if (this.currentPrivateState is ObjectManufacturerPrivateState privateStateManufacturer)
      {
        this.currentInputContainer = privateStateManufacturer.ManufacturingState.ContainerInput;
        //this.currentOutputContainer = privateStateManufacturer.ManufacturingState.ContainerOutput;
        this.currentFuelContainer = privateStateManufacturer.FuelBurningState?.ContainerFuel;
      }
    }
  }
}

