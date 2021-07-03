namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.GameApi.Data.State;

  public class EventDropPublicState : EventWithAreaPublicState
  {
    [SyncToClient]
    public byte ObjectsRemains { get; set; }

    [SyncToClient]
    public byte ObjectsTotal { get; set; }

    [SyncToClient]
    public byte NextWave { get; set; }

    [SyncToClient]
    public byte CurrentWave { get; set; }
  }
}