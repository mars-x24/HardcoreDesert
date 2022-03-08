namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  using AtomicTorch.CBND.CoreMod.Items;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Items.Seeds;
  using AtomicTorch.CBND.CoreMod.Rates;
  using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using JetBrains.Annotations;
  using System;
  using System.Linq;

  public class ObjectTradingStationLargeSeed : ObjectTradingStationLargeFridge
  {
    public override string Name => "Seeds for sale!";

    public override float StructurePointsMax => 0; // non-damageable

    public double ServerUpdateItemsIntervalSeconds => 3600;

    public override bool IsRelocatable => false;

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      if(data.PrivateState.Owners.Count == 0)
        data.PrivateState.Owners.Add("");

      this.CreateRandomLots(data.GameObject, data.PublicState, data.PrivateState);
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      data.PrivateState.UpdateItemsDeltaTime += data.DeltaTime;

      if (data.PrivateState.UpdateItemsDeltaTime > ServerUpdateItemsIntervalSeconds)
      {
        data.PrivateState.UpdateItemsDeltaTime = 0;

        this.CreateRandomLots(data.GameObject, data.PublicState, data.PrivateState);
      }

      base.ServerUpdate(data);
    }

    private void CreateRandomLots(IStaticWorldObject tradingStation, ObjectTradingStationPublicState publicState, ObjectTradingStationPrivateState privateState)
    {
      publicState.Mode = TradingStationMode.StationSelling;

      for (byte i = 0; i < privateState.StockItemsContainer.SlotsCount; i++)
      {
        var item = privateState.StockItemsContainer.GetItemAtSlot(i);
        if (item is null)
          continue;

        Server.Items.DestroyItem(item);
      }

      this.CreateLot(6, tradingStation, publicState, privateState);
    }

    private void CreateLot(byte count, IStaticWorldObject tradingStation,
      ObjectTradingStationPublicState publicState, ObjectTradingStationPrivateState privateState)
    {
      var price = Convert.ToUInt16(RateSeedTradePrice.SharedValue);

      var seeds = Api.FindProtoEntities<IProtoItemSeed>().Where(s => s is not IProtoItemSapling).ToList();
      
      if (seeds.Count == 0)
        return;

      for (int i = 0; i < count; i++)
      {
        var randomSeed = seeds[RandomHelper.Next(0, seeds.Count)];
        seeds.Remove(randomSeed);
        if (seeds.Count == 0)
          break;

        TradingStationLot lot;
        if (publicState.Lots.Count > i)
        {
          lot = publicState.Lots[i];
        }
        else
        {
          lot = new TradingStationLot();
          publicState.Lots.Add(lot);
        }
        
        lot.ProtoItem = randomSeed;
        lot.SetLotQuantity(1);
        lot.SetPrices(price, 0);
        lot.State = TradingStationLotState.Available;
        
        Server.Items.CreateItem(randomSeed, privateState.StockItemsContainer, count: 5);
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