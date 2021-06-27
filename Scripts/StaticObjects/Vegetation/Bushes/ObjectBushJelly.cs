namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;

    public class ObjectBushJelly : ProtoObjectBush
    {
        public const string ErrorNoFruit = "No fruit!";

        public override bool HasIncreasedScopeSize => true; // necessary as this bush has a large light source

        public override string Name => "Glowing berries bush";

        protected override string InteractionFailedNoFruitsMessage => ErrorNoFruit;

        protected override TimeSpan TimeToGiveHarvest => TimeSpan.FromHours(3);

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.Scale = 1f;
            renderer.PositionOffset += (0, -0.08);
            renderer.DrawOrderOffsetY = 0.3;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var clientState = data.ClientState;
            var sceneObject = data.GameObject.ClientSceneObject;

            clientState.RendererLight = ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.BioLuminescencePink,
                size: 12,
                logicalSize: 5.5,
                positionOffset: (0.5, 0.7));

            void RefreshLightSourceSpot()
            {
                // display light only if the bush has fruit
                clientState.RendererLight.IsEnabled = publicState.HasHarvest;
            }

            publicState.ClientSubscribe(
                _ => _.HasHarvest,
                _ => RefreshLightSourceSpot(),
                data.ClientState);

            RefreshLightSourceSpot();
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);

            // update light opacity accordingly to the time of day
            data.ClientState.RendererLight.Opacity
                = Math.Min(1,
                           ClientTimeOfDayVisualComponent.CurrentNightFraction
                           + ClientTimeOfDayVisualComponent.CurrentDuskDawnFraction);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 5,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemBerriesJelly>(count: 1, countRandom: 3)
                .Add<ItemBerriesJelly>(count: 2, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }
		
		protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.39,
                    center: (0.5, 0.42))
                .AddShapeRectangle(
                    size: (0.8, 0.7),
                    offset: (0.1, 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.9, 0.8),
                    offset: (0.05, 0.05),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(
                    radius: 0.4,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }
    }
}