namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.ItemContainers.Items;
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using System;
  using System.Collections.Generic;
  using System.Windows.Controls;

  public abstract class ProtoItemStorage
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
           TPublicState,
           TClientState>,
           IProtoItemStorage
        where TPrivateState : ItemStoragePrivateState, new()
        where TPublicState : ItemStoragePublicState, new()
        where TClientState : BaseClientState, new()
  {
    protected virtual IProtoItemsContainer ItemsContainerType
    => Api.GetProtoEntity<ItemsContainerStorage>();

    public abstract byte SlotsCount { get; }

    public string ItemUseCaption => "";

    public override double GroundIconScale => 1.5;

    public override ushort MaxItemsPerStack => 1;

    public override string Description => "Useful for storing food, medicines or coins.";

    public virtual void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
    {
      controls.Add(ItemSlotStorageIconOverlayControl.Create(item));
    }

    protected override void ClientItemUseStart(ClientItemData data)
    {
      base.ClientItemUseStart(data);

      this.CallServer(_ => _.ServerRemote_Use(data.Item));
    }

    protected override bool ClientItemUseFinish(ClientItemData data)
    {
      return true;
    }

    private void ServerRemote_Use(IItem item)
    {
      var character = ServerRemoteContext.Character;

      this.ServerValidateItemForRemoteCall(item, character);
 
      this.ServerInitContainer(item, character);  

      this.CallClient(character, _ => _.ClientRemote_OpenWindow(item));
    }

    protected virtual void ServerInitContainer(IItem item, ICharacter character)
    {
      var privateState = item.GetPrivateState<ItemStoragePrivateState>();
      var itemsContainer = privateState.ItemsContainer;
      if (itemsContainer is null)
      {
        itemsContainer = Server.Items.CreateContainer(
            owner: privateState.GameObject,
            itemsContainerType: this.ItemsContainerType,
            slotsCount: this.SlotsCount);

        privateState.ItemsContainer = itemsContainer;
      }

      Api.Server.World.ForceEnterScope(character, itemsContainer);
      Api.Server.World.EnterPrivateScope(character, itemsContainer);
    }

    private void ClientRemote_OpenWindow(IItem item)
    {
      this.ClientOpenWindow(item);
    }

    protected virtual void ClientOpenWindow(IItem item)
    {
      WindowStorageContainer.Open(item);
    }


    public override void ServerOnDestroy(IItem gameObject)
    {
      base.ServerOnDestroy(gameObject);

      if(gameObject.Container is null)
      {
        return;
      }

      ICharacter character = gameObject.Container.OwnerAsCharacter;
      if (character is null)
        return;

      var playerPrivateState = PlayerCharacter.GetPrivateState(character);

      IItemsContainer objectGroundContainer = null;

      objectGroundContainer = ObjectPlayerLootContainer.ServerTryCreateLootContainer(character);

      var privateState = GetPrivateState(gameObject);

      if (objectGroundContainer is not null)
      {
        int slotCount = privateState.ItemsContainer.OccupiedSlotsCount;

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

      Api.Server.Items.TryMoveAllItems(privateState.ItemsContainer, objectGroundContainer);

      WorldObjectClaimSystem.ServerTryClaim(objectGroundContainer.OwnerAsStaticObject,
                                            character,
                                            durationSeconds: objectGroundContainer.OwnerAsStaticObject.ProtoStaticWorldObject
                               is ObjectPlayerLootContainer
                               ? ObjectPlayerLootContainer.AutoDestroyTimeoutSeconds + (10 * 60)
                               : WorldObjectClaimDuration.DroppedGoods);
    }

    public void ClientSetIconSource(IItem itemStorage, IProtoEntity iconSource)
    {
      var publicState = itemStorage.GetPublicState<ItemStoragePublicState>();
      if (publicState.IconSource == iconSource)
      {
        return;
      }

      this.CallServer(_ => _.ServerRemote_SetIconSource(itemStorage, iconSource));
    }

    [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 1.5, keyArgIndex: 0)]
    private void ServerRemote_SetIconSource(IItem itemStorage, IProtoEntity iconSource)
    {
      var publicState = itemStorage.GetPublicState<ItemStoragePublicState>();
      publicState.IconSource = iconSource;
    }

    protected override void PrepareHints(List<string> hints)
    {
      base.PrepareHints(hints);
      hints.Add(ItemHints.AltClickToUseItem);
    }
  }

  public abstract class ProtoItemStorage
  : ProtoItemStorage<
      ItemStoragePrivateState,
      ItemStoragePublicState,
      EmptyClientState>
  {
  }
}