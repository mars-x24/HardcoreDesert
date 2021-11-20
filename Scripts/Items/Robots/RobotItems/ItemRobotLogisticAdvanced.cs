namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.Robots;

  public class ItemRobotLogisticAdvanced : ProtoItemRobot<RobotLogisticAdvanced>
  {
    public override string Description =>
        "Advanced version of the logistic robot offers increased speed and cargo, as well as other improvements.";

    public override uint DurabilityMax => 10000;

    public override string Name => "Advanced logistic robot";

    public override byte ItemDeliveryCount => 2;

    public override ushort DeliveryTimerSeconds => 10;
  }
}