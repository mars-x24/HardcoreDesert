namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using System;
  using System.Threading.Tasks;

  public class ItemsContainerGlobalStorage : ProtoItemsContainer
  {
    private static ItemsContainerGlobalStorage instance;

    private static bool isCompactingNow;

    static ItemsContainerGlobalStorage()
    {
      if (IsClient)
      {
        return;
      }

      ServerGlobalStorageItemsSlotsCapacity = RateGlobalStorageCapacity.SharedValue;
    }

    public ItemsContainerGlobalStorage()
    {
      instance = this;
    }

    public static event Action ClientGlobalStorageItemsSlotsCapacityChanged;

    public static byte ClientGlobalStorageItemsSlotsCapacity { get; private set; }

    public static byte ServerGlobalStorageItemsSlotsCapacity { get; }

    public override bool CanAddItem(CanAddItemContext context)
    {
      if (context.ByCharacter is null
          || isCompactingNow)
      {
        return true;
      }

      // prohibit adding items to this container by any character if its capacity is exceeded
      if (context.Container.SlotsCount
          > (IsServer
                 ? ServerGlobalStorageItemsSlotsCapacity
                 : ClientGlobalStorageItemsSlotsCapacity))
      {
        return false;
      }

      return true;
    }

    public override void ServerOnItemRemoved(IItemsContainer container, IItem item, ICharacter character)
    {
      base.ServerOnItemRemoved(container, item, character);

      if (isCompactingNow
          || container.SlotsCount <= ServerGlobalStorageItemsSlotsCapacity)
      {
        return;
      }

      // Invoke the container compacting a bit later.
      // Required when player is invoking a batch items move
      // (so we wait until the batched operations are completed).
      ServerTimersSystem.AddAction(
          delaySeconds: 0.05,
          () => ServerTryCompactContainer(container));
    }

    private static void ServerTryCompactContainer(IItemsContainer container)
    {
      if (isCompactingNow
          || container.IsDestroyed)
      {
        return;
      }

      var slotsCount = container.SlotsCount;
      if (slotsCount <= ServerGlobalStorageItemsSlotsCapacity)
      {
        return;
      }

      try
      {
        isCompactingNow = true;
        using var tempList = Api.Shared.WrapInTempList(container.Items);
        Server.Items.ReorderItems(container, tempList.AsList());
        Server.Items.SetSlotsCount(container,
                                   slotsCount: Math.Max(container.OccupiedSlotsCount,
                                                        ServerGlobalStorageItemsSlotsCapacity));
      }
      finally
      {
        isCompactingNow = false;
      }
    }

    [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
    private byte ServerRemote_RequestGlobalStorageItemsSlotsCapacity()
    {
      return ServerGlobalStorageItemsSlotsCapacity;
    }

    private class Bootstrapper : BaseBootstrapper
    {
      public override void ClientInitialize()
      {
        Client.Characters.CurrentPlayerCharacterChanged += Refresh;

        Refresh();

        void Refresh()
        {
          if (Api.Client.Characters.CurrentPlayerCharacter is not null)
          {
            instance.CallServer(_ => _.ServerRemote_RequestGlobalStorageItemsSlotsCapacity())
                    .ContinueWith(t =>
                    {
                      ClientGlobalStorageItemsSlotsCapacity = t.Result;
                      Logger.Info("Global storage slots capacity received from server: "
                                              + ClientGlobalStorageItemsSlotsCapacity);
                      Api.SafeInvoke(ClientGlobalStorageItemsSlotsCapacityChanged);
                    },
                                  TaskContinuationOptions.ExecuteSynchronously);
          }
        }
      }
    }
  }
}