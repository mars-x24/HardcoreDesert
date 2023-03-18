using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.State.NetSync;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Events
{
  public class EventWithAreaPublicState : EventPublicState
  {
    [SyncToClient]
    public Vector2Ushort AreaCirclePosition { get; set; }

    [SyncToClient]
    public ushort AreaCircleRadius { get; set; }

    [SyncToClient]
    public NetworkSyncList<string> BoundToPlayer { get; set; }

    public Vector2Ushort AreaEventOriginalPosition { get; set; }
  }
}