namespace AtomicTorch.CBND.CoreMod.Robots
{
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;

  public class ComponentRobotVisualManager : ClientComponent
  {
    public double EffectHalfDistanceIdle = 0.015;

    // When moving, the robot will float up-down faster and with a higher amplitude.
    public double EffectHalfDistanceMoving = 2.4 * 0.015;

    public double EffectSpeedIdle = 4;

    public double EffectSpeedMoving = 8;

    private double defaultOrderOffsetY;

    private double defaultPositionOffsetY;

    private Vector2D lastRobotPosition;

    private double maxVelocity;

    private IDynamicWorldObject objectRobot;

    private IComponentSpriteRenderer spriteRenderer;

    private double startTime;

    public ComponentRobotVisualManager() : base(isLateUpdateEnabled: true)
    {
    }

    public override void LateUpdate(double deltaTime)
    {
      var velocity = this.CalculateVelocity(deltaTime);
      this.lastRobotPosition = this.objectRobot.Position;

      var time = 10000.0 + ((Client.Core.ClientRealTime - this.startTime) % 10000.0);

      var offsetY = MathHelper.Lerp(
          this.EffectHalfDistanceIdle * Math.Sin(time * this.EffectSpeedIdle + MathConstants.PI),
          this.EffectHalfDistanceMoving * Math.Sin(time * this.EffectSpeedMoving + MathConstants.PI),
          this.GetMovemementSpeedCoef(velocity));

      this.spriteRenderer.DrawOrder = DrawOrder.Default;
      this.spriteRenderer.PositionOffset = (0, this.defaultPositionOffsetY + offsetY);
      this.spriteRenderer.DrawOrderOffsetY = this.defaultOrderOffsetY - this.spriteRenderer.PositionOffset.Y;
    }

    public void Setup(
        IDynamicWorldObject objectRobot,
        IComponentSpriteRenderer spriteRenderer,
        double maxVelocity)
    {
      this.objectRobot = objectRobot;
      this.spriteRenderer = spriteRenderer;
      this.defaultPositionOffsetY = spriteRenderer.PositionOffset.Y;
      this.defaultOrderOffsetY = this.spriteRenderer.DrawOrderOffsetY;
      this.maxVelocity = maxVelocity;
      this.startTime = Client.Core.ClientRealTime + RandomHelper.NextDouble();
    }

    // calculate approximated velocity by using the position difference
    private Vector2D CalculateVelocity(double deltaTime)
    {
      if (deltaTime <= 0)
      {
        return Vector2D.Zero;
      }

      var currentPosition = this.objectRobot.Position;
      var velocity = (currentPosition - this.lastRobotPosition) / deltaTime;
      return velocity;
    }

    private double GetMovemementSpeedCoef(Vector2D velocity)
    {
      var length = velocity.Length;
      length /= this.maxVelocity;
      return Math.Min(1, length);
    }
  }
}