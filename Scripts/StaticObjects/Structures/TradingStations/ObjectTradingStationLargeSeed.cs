namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  using AtomicTorch.CBND.CoreMod.Items;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Seeds;
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using JetBrains.Annotations;
  using System;

  public class ObjectTradingStationLargeSeed : ObjectTradingStationLarge
  {
    public override string Name => "Seeds for sale!";

    public override float StructurePointsMax => 0; // non-damageable

    public override double ServerUpdateIntervalSeconds => 30; //10800;

    public override double ServerUpdateRareIntervalSeconds => this.ServerUpdateIntervalSeconds;

    public override bool IsRelocatable => false;

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      this.CreateRandomLots(data.PublicState);
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      this.CreateRandomLots(data.PublicState);

      base.ServerUpdate(data);
    }

    private void CreateRandomLots(ObjectTradingStationPublicState publicState)
    {
      publicState.Mode = TradingStationMode.StationSelling;

      publicState.Lots.Clear();

      this.CreateLot(6, publicState);
    }

    private void CreateLot(byte count, ObjectTradingStationPublicState publicState)
    {
      var price = Convert.ToUInt16(RateSeedTradePrice.SharedValue);

      var seeds = Api.FindProtoEntities<IProtoItemSeed>();
      if (seeds.Count == 0)
        return;

      for (int i = 0; i < count; i++)
      {
        var lot = new TradingStationLot();
        lot.ProtoItem = seeds[RandomHelper.Next(0, seeds.Count)];
        lot.SetLotQuantity(1);
        lot.SetPrices(price, 0);
        lot.State = TradingStationLotState.Available;

        publicState.Lots.Add(lot);
      }
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

      Api.Logger.Important("ObjectTradingStationLargeSeed was destroyed");
    }
  }
}