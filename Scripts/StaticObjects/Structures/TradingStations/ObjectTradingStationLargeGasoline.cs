using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Rates;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using JetBrains.Annotations;
using System;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  public class ObjectTradingStationLargeGasoline : ObjectTradingStationLargeFridge
  {
    public override string Name => "Gasoline needed!";

    public override float StructurePointsMax => 0; // non-damageable

    public double ServerUpdateItemsIntervalSeconds => 3600;

    public override bool IsRelocatable => false;

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      if (data.PrivateState.Owners.Count == 0)
        data.PrivateState.Owners.Add("");

      this.CreateLots(data.PublicState);
      this.CreateItems(data.GameObject, data.PrivateState);
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      data.PrivateState.UpdateItemsDeltaTime += data.DeltaTime;

      if (data.PrivateState.UpdateItemsDeltaTime > ServerUpdateItemsIntervalSeconds)
      {
        data.PrivateState.UpdateItemsDeltaTime = 0;

        this.CreateItems(data.GameObject, data.PrivateState);
      }

      base.ServerUpdate(data);
    }

    private void CreateItems(IStaticWorldObject tradingStation, ObjectTradingStationPrivateState privateState)
    {
      var container = privateState.StockItemsContainer;

      for (byte i = 0; i < container.SlotsCount; i++)
      {
        var item = container.GetItemAtSlot(i);
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

    public override bool SharedCanDeconstruct(IStaticWorldObject worldObject, ICharacter character)
    {
      return false;
    }

    public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
    {
      // no decay
    }

    protected override void ServerOnStaticObjectDamageApplied(WeaponFinalCache weaponCache, IStaticWorldObject targetObject, float previousStructurePoints, float currentStructurePoints)
    {

    }

    public override bool SharedOnDamage(WeaponFinalCache weaponCache, IStaticWorldObject targetObject, double damagePreMultiplier, out double obstacleBlockDamageCoef, out double damageApplied)
    {
      obstacleBlockDamageCoef = 1.0;
      damageApplied = 0.0;
      return false;
    }

    protected override void ServerOnStaticObjectZeroStructurePoints([CanBeNull] WeaponFinalCache weaponCache, [CanBeNull] ICharacter byCharacter, [NotNull] IWorldObject targetObject)
    {

    }

    public override void ServerOnDestroy(IStaticWorldObject gameObject)
    {
      base.ServerOnDestroy(gameObject);

      Api.Logger.Important("ObjectTradingStationLargeGasoline was destroyed");
    }
  }
}