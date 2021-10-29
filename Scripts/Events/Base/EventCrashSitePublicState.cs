namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;

  public class EventCrashSitePublicState : EventWithAreaPublicState
  {
    [SyncToClient]
    public bool IsSpawnCompleted { get; set; }

    [SyncToClient]
    public byte ObjectsRemains { get; set; }

    [SyncToClient]
    public byte ObjectsTotal { get; set; }

    [SyncToClient]
    public IProtoStaticWorldObject SpawnCrashProto { get; set; }
  }
}