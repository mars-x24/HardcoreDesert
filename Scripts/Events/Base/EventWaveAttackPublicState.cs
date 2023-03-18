using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.Events
{
  public class EventWaveAttackPublicState : EventWithAreaPublicState
  {
    [SyncToClient]
    public bool IsSpawnCompleted { get; set; }

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