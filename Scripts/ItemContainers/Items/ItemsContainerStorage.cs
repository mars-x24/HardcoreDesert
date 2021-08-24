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
        return false;
      }

      if (obj is IProtoItemBackpack)
        return false;

      if (context.Container.Owner is null || context.Container.Owner.ProtoGameObject is not ProtoItemStorage)
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
      int maxItemCount = ((ProtoItemStorage)container.Owner.ProtoGameObject).MaxItemCountPerType;
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
      int maxItemCount = ((ProtoItemStorage)container.Owner.ProtoGameObject).MaxItemCount;
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
  }
}