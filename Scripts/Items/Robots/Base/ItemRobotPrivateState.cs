namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.GameApi.Data.World;

  public class ItemRobotPrivateState : ItemWithDurabilityPrivateState
  {
    public IDynamicWorldObject WorldObjectRobot { get; set; }
  }
}