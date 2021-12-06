using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace HardcoreDesert.Scripts.Systems.Robot
{
  public static class RobotTargetHelper
  {
    private static readonly Dictionary<IStaticWorldObject, IDynamicWorldObject> ServerCurrentStructureObjects = Api.IsServer ? new Dictionary<IStaticWorldObject, IDynamicWorldObject>() : null;
    private static readonly Dictionary<IItem, IDynamicWorldObject> ServerCurrentTargetedObjects = Api.IsServer ? new Dictionary<IItem, IDynamicWorldObject>() : null;

    public static bool ServerTryRegisterCurrentPickup(List<IItem> items, IStaticWorldObject structure, IDynamicWorldObject robotObject, ItemRobotPrivateState itemRobotPrivateState)
    {
      bool ok = true;

      if (ServerTryRegisterCurrentStructure(structure, robotObject, itemRobotPrivateState))
      {
        foreach (IItem item in items)
        {
          if (!ServerTryRegisterCurrentPickup(item, robotObject))
            ok = false;
        }
      }
      else
        ok = false;

      if (!ok)
        ServerUnregisterCurrentPickup(robotObject);

      return ok;
    }

    private static bool ServerTryRegisterCurrentPickup(IItem item, IDynamicWorldObject robotObject)
    {
      if (ServerCurrentTargetedObjects.TryGetValue(item, out var existingRobotObject))
      {
        return ReferenceEquals(existingRobotObject, robotObject);
      }

      ServerCurrentTargetedObjects.Add(item, robotObject);
      return true;
    }

    public static bool ServerTryRegisterCurrentStructure(IStaticWorldObject structure, IDynamicWorldObject robotObject, ItemRobotPrivateState itemRobotPrivateState)
    {
      if (itemRobotPrivateState.AllowedStructure != null && itemRobotPrivateState.AllowedStructure.Count != 0)
      {
        if (!itemRobotPrivateState.AllowedStructure.Contains(structure.ProtoGameObject))
          return false;
      }

      if (ServerCurrentStructureObjects.TryGetValue(structure, out var existingRobotObject))
      {
        return ReferenceEquals(existingRobotObject, robotObject);
      }

      ServerCurrentStructureObjects.Add(structure, robotObject);
      return true;
    }

    public static bool ServerStructureAllowed(IStaticWorldObject structure, IDynamicWorldObject robotObject, ItemRobotPrivateState itemRobotPrivateState)
    {
      if (itemRobotPrivateState.AllowedStructure != null && itemRobotPrivateState.AllowedStructure.Count != 0)
      {
        if (!itemRobotPrivateState.AllowedStructure.Contains(structure.ProtoGameObject))
          return false;
      }

      if (ServerCurrentStructureObjects.TryGetValue(structure, out var existingRobotObject))
      {
        return ReferenceEquals(existingRobotObject, robotObject);
      }

      return true;
    }

    public static void ServerUnregisterCurrentPickup(IDynamicWorldObject robotObject)
    {
      ServerCurrentStructureObjects.RemoveAllByValue(
          existingRobotObject =>
              ReferenceEquals(existingRobotObject, robotObject));

      ServerCurrentTargetedObjects.RemoveAllByValue(
          existingRobotObject =>
              ReferenceEquals(existingRobotObject, robotObject));
    }

    public static void ServerUnregisterCurrentStructure(IDynamicWorldObject robotObject)
    {
      ServerCurrentStructureObjects.RemoveAllByValue(
          existingRobotObject =>
              ReferenceEquals(existingRobotObject, robotObject));
    }

    public static bool ServerPickupAllowed(IEnumerable<IItem> items, IDynamicWorldObject robotObject)
    {
      foreach (IItem item in items)
      {
        if (ServerPickupAllowed(item, robotObject))
          return true;
      }

      return false;
    }

    public static bool ServerPickupAllowed(IItem item, IDynamicWorldObject robotObject)
    {
      if (ServerCurrentTargetedObjects.TryGetValue(item, out var existingRobotObject))
      {
        return ReferenceEquals(existingRobotObject, robotObject);
      }

      return true;
    }

    public static List<IItem> GetOutputContainersItems(IStaticWorldObject worldObject, bool includeInput)
    {
      List<IItem> items = new List<IItem>();

      if (worldObject.ProtoGameObject is ProtoObjectOilCrackingPlant)
      {
        var privateStateOil = worldObject.GetPrivateState<ProtoObjectOilCrackingPlant.PrivateState>();
        if (privateStateOil is not null)
        {
          items.AddRange(privateStateOil.ManufacturingStateGasoline.ContainerOutput.Items);
          if (includeInput)
            items.AddRange(privateStateOil.ManufacturingStateGasoline.ContainerInput.Items);
        }
      }
      else if (worldObject.ProtoGameObject is ProtoObjectOilRefinery)
      {
        var privateStateRefinery = worldObject.GetPrivateState<ProtoObjectOilRefinery.PrivateState>();
        if (privateStateRefinery is not null)
        {
          items.AddRange(privateStateRefinery.ManufacturingStateGasoline.ContainerOutput.Items);
          if (includeInput)
            items.AddRange(privateStateRefinery.ManufacturingStateGasoline.ContainerInput.Items);

          items.AddRange(privateStateRefinery.ManufacturingStateMineralOil.ContainerOutput.Items);
          if (includeInput)
            items.AddRange(privateStateRefinery.ManufacturingStateMineralOil.ContainerInput.Items);
        }
      }

      var privateStateManufacturer = worldObject.GetPrivateState<ObjectManufacturerPrivateState>();

      if (privateStateManufacturer.ManufacturingState is not null)
      {
        items.AddRange(privateStateManufacturer.ManufacturingState.ContainerOutput.Items);
        if (includeInput)
          items.AddRange(privateStateManufacturer.ManufacturingState.ContainerInput.Items);
      }

      return items.OrderBy(i => i.Count).Reverse().ToList();
    }

    public static List<IItemsContainer> GetInputContainers(IStaticWorldObject worldObject)
    {
      List<IItemsContainer> list = new List<IItemsContainer>();

      if (worldObject.ProtoGameObject is ProtoObjectOilCrackingPlant)
      {
        var privateStateOil = worldObject.GetPrivateState<ProtoObjectOilCrackingPlant.PrivateState>();
        if (privateStateOil is not null)
          list.Add(privateStateOil.ManufacturingStateGasoline.ContainerInput);
      }
      else if (worldObject.ProtoGameObject is ProtoObjectOilRefinery)
      {
        var privateStateRefinery = worldObject.GetPrivateState<ProtoObjectOilRefinery.PrivateState>();
        if (privateStateRefinery is not null)
        {
          list.Add(privateStateRefinery.ManufacturingStateGasoline.ContainerInput);
          list.Add(privateStateRefinery.ManufacturingStateMineralOil.ContainerInput);
        }
      }

      var privateStateManufacturer = worldObject.GetPrivateState<ObjectManufacturerPrivateState>();

      if (privateStateManufacturer.ManufacturingState is not null)
        list.Add(privateStateManufacturer.ManufacturingState.ContainerInput);

      return list.OrderBy(i => i.Items.Sum(i => i.Count)).ToList();
    }

    public static List<IItemsContainer> GetFuelContainers(IStaticWorldObject worldObject)
    {
      List<IItemsContainer> list = new List<IItemsContainer>();

      var privateStateManufacturer = worldObject.GetPrivateState<ObjectManufacturerPrivateState>();

      if (privateStateManufacturer.FuelBurningState is not null)
        list.Add(privateStateManufacturer.FuelBurningState.ContainerFuel);

      return list;
    }

    public static Vector2D GetTargetPosition(IWorldObject targetWorldObject)
    {
      if (targetWorldObject is not IStaticWorldObject targetStaticWorldObject)
        return (0, 0);

      var protoStaticWorldObject = targetStaticWorldObject.ProtoStaticWorldObject;
      var centerOffset = protoStaticWorldObject.SharedGetObjectCenterWorldOffset(targetWorldObject);

      return centerOffset;
    }
  }
}
