namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ObjectTradingStationFridgePublicState : ObjectTradingStationPublicState, IObjectElectricityConsumerPublicState
  {
    [SyncToClient]
    public ElectricityConsumerState ElectricityConsumerState { get; set; }
  }
}