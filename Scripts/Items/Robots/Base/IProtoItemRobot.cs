namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Robots;

  public interface IProtoItemRobot : IProtoItemWithDurability, IProtoItemUsableFromContainer
  {
    double DurabilityToStructurePointsConversionCoefficient { get; }

    IProtoRobot ProtoRobot { get; }

    byte ItemDeliveryCount { get; }

    ushort DeliveryTimerSeconds { get; }
  }
}