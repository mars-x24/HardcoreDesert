namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.ItemContainers;
  using AtomicTorch.CBND.CoreMod.Items.Equipment;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
  using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public abstract class ProtoItemBackpack
      : ProtoItemEquipmentDevice
        <ItemBackpackPrivateState,
            EmptyPublicState,
            EmptyClientState>, IProtoItemBackpack
  {
    public abstract byte SlotsCount { get; }

    public override bool IsRepairable => true;

    public const string NotificationCannotMoveEquipTitle = "Cannot move item";

    public const string NotificationCannotMoveEquip = "You have to remove items from the inventory before changing";

    //public const string NotificationCannotMoveWithBags = "You have to remove all bags from the inventory before changing";

    public override string Description => "Increase your inventory size to store your belongings.";

    public override void ServerOnItemDamaged(IItem item, double damageApplied)
    {
      ItemDurabilitySystem.ServerModifyDurability(item, delta: -(int)damageApplied);
    }

    public string ItemUseCaption => "";

    public override double GroundIconScale => 2.0;


    public override bool SharedCanApplyEffects(IItem item, IItemsContainer containerEquipment)
    {
      if (IsClient)
        return base.SharedCanApplyEffects(item, containerEquipment);

      bool active = base.SharedCanApplyEffects(item, containerEquipment);

      if (active)
        ServerSetSlots(containerEquipment.OwnerAsCharacter);

      return active;
    }

    private static void ServerSetSlots(ICharacter character)
    {
      List<IItem> list = character.SharedGetPlayerContainerEquipment().GetItemsOfProto<ProtoItemBackpack>().ToList();

      byte slotsCount = PlayerConstants.InventorySlotsCount;

      if (list.Count > 0)
        slotsCount = (byte)MathHelper.Clamp(PlayerConstants.InventorySlotsCount + ((ProtoItemBackpack)list[0].ProtoItem).SlotsCount, PlayerConstants.InventorySlotsCount, 255);

      var privateState = PlayerCharacter.GetPrivateState(character);
      if (privateState.ContainerInventory.SlotsCount != slotsCount)
      {
        Api.Server.Items.SetSlotsCount(privateState.ContainerInventory, slotsCount);
        Api.Server.Items.ServerForceContainersResync(character);
      }
    }

    public bool SharedCanRemoveItem(IItem item, ICharacter character, bool testOnly)
    {
      if (character is not null)
      {
        var privateState = PlayerCharacter.GetPrivateState(character);

        //bool containsBag = privateState.ContainerInventory.GetItemsOfProto<ProtoItemStorage>().ToList().Count > 0;

        bool canRemove = true;// !containsBag;

        if (canRemove)
        {
          for (byte i = PlayerConstants.InventorySlotsCount - 1; i < privateState.ContainerInventory.SlotsCount; i++)
          {
            if (privateState.ContainerInventory.GetItemAtSlot(i) != null)
            {
              canRemove = false;
              break;
            }
          }
        }

        if (!testOnly)
        {
          if (IsClient && !canRemove)
          {
            NotificationSystem.ClientShowNotification(
                  NotificationCannotMoveEquipTitle,
                  NotificationCannotMoveEquip,
                  //containsBag ? NotificationCannotMoveWithBags : NotificationCannotMoveEquip,
                  NotificationColor.Bad,
                  this.Icon);
          }

          if (IsServer && canRemove)
          {
            ServerTimersSystem.AddAction(0,
                () => ServerSetSlots(character));
          }
        }

        return canRemove;
      }

      return true;
    }


    public override void ServerOnDestroy(IItem gameObject)
    {
      base.ServerOnDestroy(gameObject);

      ICharacter character = gameObject.Container.OwnerAsCharacter;
      if (character is null)
        return;

      var playerPrivateState = PlayerCharacter.GetPrivateState(character);

      IItemsContainer objectGroundContainer = null;

      objectGroundContainer = ObjectPlayerLootContainer.ServerTryCreateLootContainer(character);

      if (objectGroundContainer is not null)
      {
        int slotCount = this.GetGroundSlotCount(playerPrivateState);

        // set slots count matching the total occupied slots count
        Server.Items.SetSlotsCount(objectGroundContainer,
            (byte)Math.Min(byte.MaxValue, objectGroundContainer.OccupiedSlotsCount + slotCount));
      }

      if (objectGroundContainer is null)
      {
        objectGroundContainer = ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(character, character.Tile);

        if (objectGroundContainer is null)
          return;
      }

      for (byte i = PlayerConstants.InventorySlotsCount; i < playerPrivateState.ContainerInventory.SlotsCount; i++)
      {
        IItem itemToDrop = playerPrivateState.ContainerInventory.GetItemAtSlot(i);
        if (itemToDrop is not null)
        {
          Server.Items.MoveOrSwapItem(itemToDrop, objectGroundContainer, out _);
        }
      }

      if (playerPrivateState.ContainerInventory.SlotsCount != PlayerConstants.InventorySlotsCount)
      {
        Api.Server.Items.SetSlotsCount(playerPrivateState.ContainerInventory, PlayerConstants.InventorySlotsCount);
      }

      WorldObjectClaimSystem.ServerTryClaim(objectGroundContainer.OwnerAsStaticObject,
                                            character,
                                            durationSeconds: objectGroundContainer.OwnerAsStaticObject.ProtoStaticWorldObject
                               is ObjectPlayerLootContainer
                               ? ObjectPlayerLootContainer.AutoDestroyTimeoutSeconds + (10 * 60)
                               : WorldObjectClaimDuration.DroppedGoods);
    }

    private int GetGroundSlotCount(PlayerCharacterPrivateState playerPrivateState)
    {
      int nb = 0;

      for (byte i = PlayerConstants.InventorySlotsCount; i < playerPrivateState.ContainerInventory.SlotsCount; i++)
      {
        IItem itemToDrop = playerPrivateState.ContainerInventory.GetItemAtSlot(i);
        if (itemToDrop is not null)
        {
          nb++;
        }
      }

      return nb;
    }

    public static bool SharedCanAddItem(CanAddItemContext context)
    {
      if (!context.SlotId.HasValue)
        return true;

      var slotIdValue = context.SlotId.Value;
      if (slotIdValue < (byte)EquipmentType.Device || slotIdValue > (byte)EquipmentType.Device + 4)
        return true;

      IItem itemAtSlot = context.Container.GetItemAtSlot(slotIdValue);
      if (itemAtSlot is not null && itemAtSlot.ProtoItem is ProtoItemBackpack backpack)
      {
        return backpack.SharedCanRemoveItem(itemAtSlot, context.ByCharacter, true);
      }

      if (context.Item.ProtoItem is ProtoItemBackpack && context.ByCharacter is not null)
      {
        var countE = context.ByCharacter.SharedGetPlayerContainerEquipment().GetItemsOfProto<ProtoItemBackpack>().ToList().Count;
        if (countE >= 1)
          return false;
      }

      return true;
    }

  }
}