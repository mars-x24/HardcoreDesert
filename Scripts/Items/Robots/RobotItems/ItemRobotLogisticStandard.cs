﻿namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.Robots;

  public class ItemRobotLogisticStandard : ProtoItemRobot<RobotLogisticStandard>
  {
    public override string Description => "Standard logistic robot can be used for quick item acquisition.";

    public override uint DurabilityMax => 7500;

    public override string Name => "Logistic robot";

  }
}