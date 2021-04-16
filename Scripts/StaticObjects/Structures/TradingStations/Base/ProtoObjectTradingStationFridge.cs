namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
  using AtomicTorch.CBND.GameApi.Data.World;

  public abstract class ProtoObjectTradingStationFridge
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectTradingStation
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectFridge
        where TPrivateState : ObjectTradingStationPrivateState, new()
        where TPublicState : ObjectTradingStationPublicState, new()
        where TClientState : ObjectTradingStationClientState, new()
  {
    public abstract double FreshnessDurationMultiplier { get; }

    public virtual double ServerGetCurrentFreshnessDurationMultiplier(IStaticWorldObject worldObject)
    {
      return this.FreshnessDurationMultiplier;
    }
  }

  public abstract class ProtoObjectTradingStationFridge
      : ProtoObjectTradingStationFridge<
          ObjectTradingStationPrivateState,
          ObjectTradingStationPublicState,
          ObjectTradingStationClientState>
  {
  }
}