using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.ItemContainers;
using AtomicTorch.CBND.CoreMod.ItemContainers.Items;
using AtomicTorch.CBND.CoreMod.Items.Devices;
using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  public abstract class ProtoItemStorageFridge
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemStorage
          <TPrivateState,
           TPublicState,
           TClientState>
        where TPrivateState : ItemStorageFridgePrivateState, new()
        where TPublicState : ItemStorageFridgePublicState, new()
        where TClientState : BaseClientState, new()
  {
    public override string Description
      => "Can store food at below-zero temperature, ensuring that it stays fresh much longer.";

    public const string HintUsesPowerBanks = "This item draws power from his own equipped powerbank.";

    public const string HintFreshness = "Perishable items store ~{0} times longer.";

    protected override IProtoItemsContainer ItemsContainerType => Api.GetProtoEntity<ItemsContainerStorageFridge>();

    public abstract byte SlotsEnergyCount { get; }

    private IProtoItemsContainer ItemsEnergyContainerType => Api.GetProtoEntity<ItemsContainerPowerBank>();

    public override double ServerUpdateIntervalSeconds => 0.1;

    public abstract double FreshnessDurationMultiplier { get; }

    public abstract double EnergyConsumptionPerSecond { get; }

    public override void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
    {
      controls.Add(ItemSlotStorageFridgeIconOverlayControl.Create(item));

      base.ClientCreateItemSlotOverlayControls(item, controls);  
    }

    public virtual double GetCurrentFreshnessDurationMultiplier(ItemStorageFridgePublicState publicState)
    {
      if (!publicState.IsOn)
      {
        // no power supplied so no freshness increase
        return 1;
      }

      return this.FreshnessDurationMultiplier;
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      var item = data.GameObject;
      var publicState = data.PublicState;
      var privateState = data.PrivateState;

      this.UpdatePower(item, publicState, privateState, publicState.IsOn);

      publicState.IconOverlay = ((TextureResource)this.ClientGetIcon(item)).FullPath;
    }

    public void ServerRemote_ApplyClientIsOn(IItem itemStorage, bool isOn)
    {
      var publicState = itemStorage.GetPublicState<ItemStorageFridgePublicState>();

      var privateState = itemStorage.GetPrivateState<ItemStorageFridgePrivateState>();

      this.UpdatePower(itemStorage, publicState, privateState, isOn);
    }

    private void UpdatePower(IItem item, ItemStorageFridgePublicState publicState, ItemStorageFridgePrivateState privateState, bool isOn)
    {
      if (item.Container is null)
        return;

      var character = item.Container.OwnerAsCharacter;

      if (isOn)
      {
        publicState.IsOn = true; //force the client to refresh if isOn = false and stays false

        (double energyBefore, double totalEnergyMaxBefore) = this.SharedCalculateTotalEnergyCharge(item);
        publicState.LastPowerPourcent = (double)energyBefore / totalEnergyMaxBefore;

        publicState.IsOn = ServerDeductEnergyCharge(
              character,
              requiredEnergyAmount: this.EnergyConsumptionPerSecond * ServerUpdateIntervalSeconds,
              publicState, privateState);

        (double energyAfter, double totalEnergyMaxAfter) = this.SharedCalculateTotalEnergyCharge(item);
        publicState.PowerPourcent = (double)energyAfter / totalEnergyMaxAfter;
      }
      else
      {
        publicState.IsOn = false;
      }
    }


    private bool ServerDeductEnergyCharge(ICharacter character, double requiredEnergyAmount,
      ItemStorageFridgePublicState publicState, ItemStorageFridgePrivateState privateState)
    {
      if (requiredEnergyAmount <= 0)
      {
        return true;
      }

      List<IItem> list = privateState.ItemsEnergyContainer.GetItemsOfProto<IProtoItemPowerBank>().ToList();

      if (list.Count == 0)
      {
        // there are no battery packs equipped
        return false;
      }

      // deduct energy in reverse order
      for (var index = list.Count - 1; index >= 0; index--)
      {
        var item = list[index];
        var powerBankPrivateState = list[index].GetPrivateState<ItemPowerBankPrivateState>();
        var charge = powerBankPrivateState.EnergyCharge;
        if (charge <= 0)
        {
          // no charge there
          continue;
        }

        if (charge >= requiredEnergyAmount)
        {
          // there are more than enough charge available
          powerBankPrivateState.EnergyCharge -= requiredEnergyAmount;
          ServerOnEnergyUsed(item, requiredEnergyAmount);
          return true;
        }

        // use all the remaining charge in this item
        requiredEnergyAmount -= charge;
        powerBankPrivateState.EnergyCharge = 0;
        ServerOnEnergyUsed(item, charge);
      }

      return false;
    }

    private (double, double) SharedCalculateTotalEnergyCharge(IItem itemStorage)
    {
      var privateStateItemStorage = itemStorage.GetPrivateState<ItemStorageFridgePrivateState>();
      List<IItem> tempItemsList = privateStateItemStorage.ItemsEnergyContainer.GetItemsOfProto<IProtoItemPowerBank>().ToList();

      var result = 0.0;
      var resultMax = 0.0;

      foreach (var item in tempItemsList)
      {
        var privateState = item.GetPrivateState<ItemPowerBankPrivateState>();
        var charge = privateState.EnergyCharge;
        result += charge;
        resultMax += ((IProtoItemPowerBank)item.ProtoItem).EnergyCapacity;
      }

      return (result, resultMax);
    }


    private static void ServerOnEnergyUsed(IItem item, double energyAmountUsed)
    {
      if (energyAmountUsed <= 0)
      {
        return;
      }

      // reduce durability proportionally to the removed charge, durability as int is dropping to fast
      if(RandomHelper.Next(20) == 1)
        ItemDurabilitySystem.ServerModifyDurability(item, -energyAmountUsed, true);
    }


    protected override void ClientOpenWindow(IItem item)
    {
      WindowStorageFridgeContainer.Open(item);
    }

    protected override void ServerInitContainer(IItem item, ICharacter character)
    {
      base.ServerInitContainer(item, character);

      var privateState = item.GetPrivateState<ItemStorageFridgePrivateState>();
      var energyItemsContainer = privateState.ItemsEnergyContainer;
      if (energyItemsContainer is null)
      {
        energyItemsContainer = Server.Items.CreateContainer(
            owner: privateState.GameObject,
            itemsContainerType: this.ItemsEnergyContainerType,
            slotsCount: this.SlotsEnergyCount);

        privateState.ItemsEnergyContainer = energyItemsContainer;
      }
    }

    public static bool SharedCanAddItem(CanAddItemContext context)
    {
      if (context.Item.ProtoItem is ProtoItemStorageFridge)
      {
        List<IItem> listI = context.Container.OwnerAsCharacter.SharedGetPlayerContainerInventory().GetItemsOfProto<ProtoItemStorageFridge>().ToList();
        listI.Remove(context.Item);

        List<IItem> listH = context.Container.OwnerAsCharacter.SharedGetPlayerContainerHotbar().GetItemsOfProto<ProtoItemStorageFridge>().ToList();
        listH.Remove(context.Item);

        if (listI.Count + listH.Count >= 1)
          return false;
      }

      return true;
    }

    protected override void PrepareHints(List<string> hints)
    {
      base.PrepareHints(hints);
      hints.Add(HintUsesPowerBanks);
      hints.Add(string.Format(HintFreshness, this.FreshnessDurationMultiplier));
    }


  }

  public abstract class ProtoItemStorageFridge
    : ProtoItemStorageFridge<
        ItemStorageFridgePrivateState,
        ItemStorageFridgePublicState,
        EmptyClientState>
  {
  }
}