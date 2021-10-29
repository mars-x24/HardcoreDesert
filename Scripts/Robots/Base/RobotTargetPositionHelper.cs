namespace AtomicTorch.CBND.CoreMod.Robots
{
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.GameEngine.Common.Primitives;

  public static class RobotTargetPositionHelper
  {
    public static Vector2D GetTargetPosition(IWorldObject targetWorldObject)
    {
      if (targetWorldObject is not IStaticWorldObject targetStaticWorldObject)
        return (0,0);

      var protoStaticWorldObject = targetStaticWorldObject.ProtoStaticWorldObject;
      var centerOffset = protoStaticWorldObject.SharedGetObjectCenterWorldOffset(targetWorldObject);

      return centerOffset;
    }
  }
}