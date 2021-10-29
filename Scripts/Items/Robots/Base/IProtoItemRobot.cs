namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.Robots;

  public interface IProtoItemRobot : IProtoItemWithDurability
  {
    double DurabilityToStructurePointsConversionCoefficient { get; }

    IProtoRobot ProtoRobot { get; }
  }
}