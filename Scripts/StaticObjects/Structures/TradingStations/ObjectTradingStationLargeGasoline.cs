namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  using AtomicTorch.CBND.CoreMod.Items;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System;

  public class ObjectTradingStationLargeGasoline : ObjectTradingStationLarge
  {
    public override string Name => "Gasoline needed!";

    public override float StructurePointsMax => 0; // non-damageable

    public override double ServerUpdateIntervalSeconds => 10800;

    public override double ServerUpdateRareIntervalSeconds => this.ServerUpdateIntervalSeconds;

    public override bool IsRelocatable => false;

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      this.CreateLots(data.PublicState);
      this.CreateItems(data.PrivateState);
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      this.CreateItems(data.PrivateState);

      base.ServerUpdate(data);
    }

    private void CreateItems(ObjectTradingStationPrivateState privateState)
    {
      var container = privateState.StockItemsContainer;

      for (byte i = 0; i < container.SlotsCount; i++)
      {
        var item = privateState.StockItemsContainer.GetItemAtSlot(i);
        if (item is null)
          continue;

        Server.Items.DestroyItem(item);
      }

      var pennies = RateGasolineCanisterTradePrice.SharedValue * 1000 / ItemStackSize.Huge;

      for (byte i = 0; i < container.SlotsCount - 3; i++)
      {
        if (container.IsSlotOccupied(i))
          continue;

        Server.Items.CreateItem<ItemCoinPenny>(container, count: ItemStackSize.Huge, slotId: i);

        pennies--;
        if (pennies == 0)
          break;
      }
    }

    private void CreateLots(ObjectTradingStationPublicState publicState)
    {
      publicState.Mode = TradingStationMode.StationBuying;

      publicState.Lots.Clear();

      this.CreateLot(1, publicState);
      this.CreateLot(10, publicState);
      this.CreateLot(25, publicState);
      this.CreateLot(50, publicState);
      this.CreateLot(100, publicState);
    }

    private void CreateLot(ushort lotQuantity, ObjectTradingStationPublicState publicState)
    {
      var price = lotQuantity * RateGasolineCanisterTradePrice.SharedValue;
      if (price > ItemStackSize.Huge)
        return;

      var lot = new TradingStationLot();
      lot.ProtoItem = Api.GetProtoEntity<ItemCanisterGasoline>();
      lot.SetLotQuantity(lotQuantity);
      lot.SetPrices(Convert.ToUInt16(price), 0);
      lot.State = TradingStationLotState.Available;

      publicState.Lots.Add(lot);
    }

  }
}