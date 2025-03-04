﻿using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;

namespace AtomicTorch.CBND.CoreMod.Robots
{
  public class RobotPrivateState : BasePrivateState
  {
    public IItem AssociatedItem { get; set; }

    public IItem AssociatedItemReservedSlot { get; set; }

    public IWorldObject Owner { get; set; }

    public IItemsContainer OwnerContainer { get; set; }

    public bool IsDespawned { get; set; } = true;

    public IItemsContainer ReservedItemsContainer { get; set; }

    public IItemsContainer StorageItemsContainer { get; set; }

    [SyncToClient(
      deliveryMode: DeliveryMode.ReliableSequenced,
      maxUpdatesPerSecond: 10,
      networkDataType: typeof(double))]
    public double TimerInactive { get; set; }
  }
}