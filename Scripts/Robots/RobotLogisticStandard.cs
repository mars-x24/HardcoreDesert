namespace AtomicTorch.CBND.CoreMod.Robots
{
  using AtomicTorch.CBND.CoreMod.Items.Robots;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.GameEngine.Common.Primitives;

  public class RobotLogisticStandard : ProtoRobot<ItemRobotLogisticStandard>
  {
    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override float ObjectSoundRadius => 1;

    public override double PhysicsBodyAccelerationCoef => 3;

    public override double PhysicsBodyFriction => 30;

    public override double StatMoveSpeed => 2.0;

    public override float StructurePointsMax => 200;

    protected override double DrawVerticalOffset => 0.667;

    protected override SoundResource EngineSoundResource { get; }
        = new("Items/Robots/FlyStandard");

    protected override double EngineSoundVolume => 0.6;

    override public byte ItemDeliveryCount => 2;

    public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
    {
      return (0, 0.1 + this.DrawVerticalOffset);
    }

    protected override void PrepareProtoVehicleDestroyedExplosionPreset(
        out double damageRadius,
        out ExplosionPreset explosionPreset,
        out DamageDescription damageDescriptionCharacters)
    {
      damageRadius = 2.1;
      explosionPreset = ExplosionPresets.Large;

      damageDescriptionCharacters = new DamageDescription(
          damageValue: 50,
          armorPiercingCoef: 0,
          finalDamageMultiplier: 1,
          rangeMax: damageRadius,
          damageDistribution: new DamageDistribution(DamageType.Kinetic, 1));
    }

    protected override void SharedCreatePhysicsRobot(CreatePhysicsData data)
    {
      data.PhysicsBody
          .AddShapeRectangle(size: (0.6, 0.6), offset: (-0.3, 0.3), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6, 0.6), offset: (-0.3, 0.3), group: CollisionGroups.HitboxRanged);
    }
  }
}