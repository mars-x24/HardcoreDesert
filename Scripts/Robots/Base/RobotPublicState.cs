using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Robots
{
  public class RobotPublicState : BasePublicState, IPublicStateWithStructurePoints
  {
    [SyncToClient]
    [TempOnly]
    public bool IsGoingBackToOwner { get; set; }

    [SyncToClient]
    [TempOnly]
    public bool Loaded { get; set; }

    [SyncToClient(
        deliveryMode: DeliveryMode.ReliableSequenced,
        maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
    public float StructurePointsCurrent { get; set; }

    [SyncToClient]
    [TempOnly]
    public IStaticWorldObject Target { get; private set; }

    [SyncToClient]
    [TempOnly]
    public Dictionary<IItem, ushort> TargetItems { get; private set; }

    [SyncToClient]
    [TempOnly]
    public Dictionary<IProtoItem, ushort> InputItems { get; private set; }

    [SyncToClient]
    [TempOnly]
    public Dictionary<IProtoItem, ushort> FuelItems { get; private set; }


    public void ResetTargetPosition()
    {
      this.IsGoingBackToOwner = false;
      this.Loaded = false;

      this.Target = null;
      this.TargetItems = null;
      this.InputItems = null;
      this.FuelItems = null;
    }

    public void SetTargetPosition(IStaticWorldObject target, Dictionary<IItem, ushort> targetItems, Dictionary<IProtoItem, ushort> inputItems, Dictionary<IProtoItem, ushort> fuelItems)
    {
      this.IsGoingBackToOwner = false;
      this.Loaded = false;

      this.Target = target;
      this.TargetItems = targetItems;
      this.InputItems = inputItems;
      this.FuelItems = fuelItems;
    }

    public Dictionary<IProtoItem, ushort> ItemsToBring()
    {
      var list = new Dictionary<IProtoItem, ushort>();

      foreach (var itemProto in this.InputItems.Keys)
      {
        if (list.ContainsKey(itemProto))
          list[itemProto] = (ushort)((ushort)list[itemProto] + (ushort)this.InputItems[itemProto]);
        else
          list.Add(itemProto, (ushort)this.InputItems[itemProto]);
      }
      foreach (var itemProto in this.FuelItems.Keys)
      {
        if (list.ContainsKey(itemProto))
          list[itemProto] = (ushort)((ushort)list[itemProto] + (ushort)this.FuelItems[itemProto]);
        else
          list.Add(itemProto, (ushort)this.FuelItems[itemProto]);
      }

      return list;
    }
  }
}