namespace AtomicTorch.CBND.CoreMod.ItemContainers.Items
{
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Medical;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;

  public class ItemsContainerStorage : ProtoItemsContainer
  {
    public override bool CanAddItem(CanAddItemContext context)
    {
      var obj = context.Item.ProtoGameObject;
      var proto = context.Item.ProtoItem;

      if (obj is IProtoItemStorage)
        return false;

      if (obj is IProtoItemBackpack)
        return false;

      return obj is IProtoItemFood || obj is IProtoItemMedical || proto is ItemCoinShiny || proto is ItemCoinPenny;
    }

    public override void SharedValidateCanInteract(ICharacter character, IItemsContainer container, bool writeToLog)
    {
      //don't call base - non-world error
    }
  }
}