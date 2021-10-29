namespace AtomicTorch.CBND.CoreMod.Robots
{
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Collections.Generic;

  public class RobotPublicState : BasePublicState, IPublicStateWithStructurePoints
  {
    [SyncToClient]
    [TempOnly]
    public bool IsGoingBackToOwner { get; set; }

    [SyncToClient(
        deliveryMode: DeliveryMode.ReliableSequenced,
        maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
    public float StructurePointsCurrent { get; set; }

    [SyncToClient]
    [TempOnly]
    public IStaticWorldObject Target { get; private set; }

    [SyncToClient]
    [TempOnly]
    public List<IItem> TargetItems { get; private set; }


    public void ResetTargetPosition()
    {
      this.IsGoingBackToOwner = false;

      this.Target = null;
      this.TargetItems = null;
    }

    public void SetTargetPosition(IStaticWorldObject target, List<IItem> targetItems)
    {
      this.IsGoingBackToOwner = false;

      this.Target = target;
      this.TargetItems = targetItems;
    }
  }
}