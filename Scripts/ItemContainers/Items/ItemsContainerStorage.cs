namespace AtomicTorch.CBND.CoreMod.ItemContainers.Items
{
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Medical;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using System;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage;

  public class ItemsContainerStorage : ProtoItemsContainer
  {

    public override bool CanAddItem(CanAddItemContext context)
    {
      var obj = context.Item.ProtoGameObject;
      var proto = context.Item.ProtoItem;

      if (obj is IProtoItemStorage)
      {
        WindowStorageContainer.Close(context.Item);
        WindowStorageFridgeContainer.Close(context.Item);
        return false;
      }

      if (obj is IProtoItemBackpack)
        return false;

      if (context.Container.Owner is null || (context.Container.Owner.ProtoGameObject is not ProtoItemStorage && context.Container.Owner.ProtoGameObject is not ProtoItemStorageFridge))
        return false;

      //Waiting a better Context when the user ctrl right click an item, ai_enabled will maybe add more attributes to detect this
      //if (!CheckMaxItemCountPerTypeAllowed(context.Container, context.Item))
      //  return false;

      if (!CheckMaxItemCountAllowed(context.Container, context.Item))
        return false;

      //if (proto is ItemBottleEmpty || proto is ItemBottleWater || proto is ItemBottleWaterSalty || proto is ItemBottleWaterStale)
      //  return false;

      return obj is IProtoItemFood || obj is IProtoItemMedical || proto is ItemCoinShiny || proto is ItemCoinPenny;
    }

    public override void SharedValidateCanInteract(ICharacter character, IItemsContainer container, bool writeToLog)
    {
      //don't call base - non-world error
    }

    private bool CheckMaxItemCountPerTypeAllowed(IItemsContainer container, IItem itemToAdd)
    {
      int maxItemCount = this.GetMaxItemCountPerType(container);
      int count = itemToAdd.Count;

      if (count > maxItemCount)
        return false;

      Type type = itemToAdd.ProtoItem.GetType();

      foreach(IItem item in container.Items)
      {
        if (item.ProtoItem.GetType() == type)
          count += item.Count;

        if (count > maxItemCount)
          return false;
      }

      return true;
    }

    private bool CheckMaxItemCountAllowed(IItemsContainer container, IItem itemToAdd)
    {
      int maxItemCount = this.GetMaxItemCount(container);
      int count = itemToAdd.Count;

      if (count > maxItemCount)
        return false;

      foreach (IItem item in container.Items)
      {
        count += item.Count;
        if (count > maxItemCount)
          return false;
      }

      return true;
    }

    private int GetMaxItemCount(IItemsContainer container)
    {
      var storage = container.Owner.ProtoGameObject as ProtoItemStorage;
      if(storage is not null)
        return storage.MaxItemCount;

      var storageF = container.Owner.ProtoGameObject as ProtoItemStorageFridge;
      if (storageF is not null)
        return storageF.MaxItemCount;

      return 0;
    }

    private int GetMaxItemCountPerType(IItemsContainer container)
    {
      var storage = container.Owner.ProtoGameObject as ProtoItemStorage;
      if (storage is not null)
        return storage.MaxItemCountPerType;

      var storageF = container.Owner.ProtoGameObject as ProtoItemStorageFridge;
      if (storageF is not null)
        return storageF.MaxItemCountPerType;

      return 0;
    }
  }
}