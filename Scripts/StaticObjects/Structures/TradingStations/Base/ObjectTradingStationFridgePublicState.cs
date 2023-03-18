using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  public class ObjectTradingStationFridgePublicState : ObjectTradingStationPublicState, IObjectElectricityConsumerPublicState
  {
    [SyncToClient]
    public ElectricityConsumerState ElectricityConsumerState { get; set; }
  }
}