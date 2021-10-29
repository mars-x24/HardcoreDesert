using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace HardcoreDesert.Scripts.Robots.Base
{
  public static class RobotTargetHelper
  {
    private static readonly Dictionary<IItem, IDynamicWorldObject> ServerCurrentTargetedObjects = Api.IsServer ? new Dictionary<IItem, IDynamicWorldObject>() : null;

    public static bool ServerTryRegisterCurrentPickup(
    List<IItem> items,
    IDynamicWorldObject robotObject)
    {
      bool ok = true;

      foreach (IItem item in items)
      {
        if (!ServerTryRegisterCurrentPickup(item, robotObject))
          ok = false;
      }

      return ok;
    }

    public static bool ServerTryRegisterCurrentPickup(
        IItem item,
        IDynamicWorldObject robotObject)
    {
      if (ServerCurrentTargetedObjects.TryGetValue(item, out var existingRobotObject))
      {
        return ReferenceEquals(existingRobotObject, robotObject);
      }

      ServerCurrentTargetedObjects.Add(item, robotObject);
      return true;
    }

    public static void ServerUnregisterCurrentPickup(IDynamicWorldObject robotObject)
    {
      ServerCurrentTargetedObjects.RemoveAllByValue(
          existingRobotObject =>
              ReferenceEquals(existingRobotObject, robotObject));
    }

    public static bool ServerPickupAllowed(List<IItem> items, IDynamicWorldObject robotObject)
    {
      foreach(IItem item in items)
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

    public static List<IItem> GetOutputContainersItems(IStaticWorldObject worldObject)
    {
      List<IItem> items = new List<IItem>();

      if (worldObject.ProtoGameObject is ProtoObjectOilCrackingPlant)
      {
        var privateStateOil = worldObject.GetPrivateState<ProtoObjectOilCrackingPlant.PrivateState>();
        if (privateStateOil is not null)
          items.AddRange(privateStateOil.ManufacturingStateGasoline.ContainerOutput.Items);
      }
      else if (worldObject.ProtoGameObject is ProtoObjectOilRefinery)
      {
        var privateStateRefinery = worldObject.GetPrivateState<ProtoObjectOilRefinery.PrivateState>();
        if (privateStateRefinery is not null)
        {
          items.AddRange(privateStateRefinery.ManufacturingStateGasoline.ContainerOutput.Items);
          items.AddRange(privateStateRefinery.ManufacturingStateMineralOil.ContainerOutput.Items);
        }
      }

      var privateStateManufacturer = worldObject.GetPrivateState<ObjectManufacturerPrivateState>();

      if (privateStateManufacturer.ManufacturingState is not null)
        items.AddRange(privateStateManufacturer.ManufacturingState.ContainerOutput.Items);

      //if (privateStateManufacturer.FuelBurningByproductsQueue is not null)
     //   items.AddRange(privateStateManufacturer.FuelBurningByproductsQueue.ContainerOutput.Items);

      return items.OrderBy(i => i.Count).Reverse().ToList();
    }


  }
}
