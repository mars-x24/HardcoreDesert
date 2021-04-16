namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ObjectTradingStationFridgePrivateState : ObjectTradingStationPrivateState, IObjectElectricityStructurePrivateState
  {
    [SyncToClient]
    public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

    [SyncToClient]
    [TempOnly]
    public byte PowerGridChargePercent { get; set; }
  }
}