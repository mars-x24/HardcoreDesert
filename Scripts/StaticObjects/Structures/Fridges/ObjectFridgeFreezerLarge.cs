namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Construction;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using AtomicTorch.GameEngine.Common.Primitives;

  public class ObjectFridgeFreezerLarge : ProtoObjectFridgeElectrical
  {
    public override string Description =>
        "Extra large freezer that can store food at below-zero temperature, with optimized storage space.";

    public override double ElectricityConsumptionPerSecondWhenActive => 0.6;

    public override double FreshnessDurationMultiplier => 25;

    public override bool HasOwnersList => false;

    public override byte ItemsSlotsCount => 48;

    public override string Name => "Large freezer";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override double ObstacleBlockDamageCoef => 1;

    public override float StructurePointsMax => 5000;

    protected override Vector2D ItemIconOffset => (1, 0.138);

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.PositionOffset += (0, 0.4);
      renderer.DrawOrderOffsetY = 0.2;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("##");
    }

    protected override void PrepareConstructionConfig(
        ConstructionTileRequirements tileRequirements,
        ConstructionStageConfig build,
        ConstructionStageConfig repair,
        ConstructionUpgradeConfig upgrade,
        out ProtoStructureCategory category)
    {
      category = GetCategory<StructureCategoryStorage>();

      build.StagesCount = 5;
      build.StageDurationSeconds = BuildDuration.Short;
      build.AddStageRequiredItem<ItemPlastic>(count: 10);
      build.AddStageRequiredItem<ItemWire>(count: 10);
      build.AddStageRequiredItem<ItemComponentsElectronic>(count: 2);

      repair.StagesCount = 10;
      repair.StageDurationSeconds = BuildDuration.Short;
      repair.AddStageRequiredItem<ItemPlastic>(count: 2);
      repair.AddStageRequiredItem<ItemWire>(count: 2);
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      //data.PhysicsBody
      //    .AddShapeRectangle((0.95, 0.475), offset: (0.025, 0.4))
      //    .AddShapeRectangle((0.95, 0.75), offset: (0.025, 0.4), group: CollisionGroups.HitboxMelee)
      //    .AddShapeRectangle((0.95, 0.85), offset: (0.025, 0.4), group: CollisionGroups.ClickArea);

      data.PhysicsBody
            .AddShapeRectangle(size: (1.8, 0.475), offset: (0.1, 0.4))
            .AddShapeRectangle(size: (1.8, 0.75), offset: (0.1, 0.4), group: CollisionGroups.HitboxMelee)
            .AddShapeRectangle(size: (1.8, 0.85), offset: (0.1, 0.4), group: CollisionGroups.ClickArea);
    }
  }
}