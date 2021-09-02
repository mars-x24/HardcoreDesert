namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectCrateHugeGrounded : ProtoObjectCrate
    {
        public override string Description =>
            "Huge grounded crate for maximum storage capacity.";

        public override bool HasOwnersList => false;

        public override byte ItemsSlotsCount => 128;

        public override string Name => "Huge grounded crate";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override double StructureExplosiveDefenseCoef => 0.25;

        public override float StructurePointsMax => 1800;

        protected override Vector2D ItemIconOffset => (1, base.ItemIconOffset.Y + 0.275);

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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 8);

            repair.StagesCount = 8;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier4);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.4, 0.75), offset: (-0.2, 0.4))
                .AddShapeRectangle(size: (1.8, 0.75),  offset: (0.1, 0.4), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.8, 0.75),  offset: (0.1, 0.4), group: CollisionGroups.ClickArea);
        }
    }
}