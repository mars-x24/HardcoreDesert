﻿using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.Systems.Construction;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  public class ObjectTradingStationSmallFridge : ProtoObjectTradingStationFridgeElectrical
  {
    public override string Description =>
        "Small refrigerated automated trading station. Can be used to conveniently trade items with other survivors. Can be configured to either sell or buy orders.";

    public override byte LotsCount => 2;

    public override string Name => "Small refrigerated trading station";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override double ObstacleBlockDamageCoef => 1;

    public override byte StockItemsContainerSlotsCount => 16;

    public override double StructureExplosiveDefenseCoef => 0.25;

    public override float StructurePointsMax => 20000;

    public override double FreshnessDurationMultiplier => 25;

    public override double ElectricityConsumptionPerSecondWhenActive => 0.2;

    protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
    {
      return ClientLighting.CreateLightSourceSpot(
          sceneObject,
          color: LightColors.ElectricCold,
          size: (4, 8),
          positionOffset: (0.5, 0.6));
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      data.ClientState
          .RendererTradingStationContent
          .PositionOffset
          = (0.565, 0.11);

      ClientFixTradingStationContentDrawOffset(data);
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrderOffsetY = 0.2;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("#");
    }

    protected override void PrepareConstructionConfig(
        ConstructionTileRequirements tileRequirements,
        ConstructionStageConfig build,
        ConstructionStageConfig repair,
        ConstructionUpgradeConfig upgrade,
        out ProtoStructureCategory category)
    {
      category = GetCategory<StructureCategoryOther>();

      build.StagesCount = 10;
      build.StageDurationSeconds = BuildDuration.Short;
      build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
      build.AddStageRequiredItem<ItemGlassRaw>(count: 5);
      build.AddStageRequiredItem<ItemComponentsElectronic>(count: 3);
      build.AddStageRequiredItem<ItemPlastic>(count: 3);
      build.AddStageRequiredItem<ItemWire>(count: 3);

      repair.StagesCount = 10;
      repair.StageDurationSeconds = BuildDuration.Short;
      repair.AddStageRequiredItem<ItemIngotSteel>(count: 3);
      repair.AddStageRequiredItem<ItemPlastic>(count: 1);
      repair.AddStageRequiredItem<ItemWire>(count: 1);
    }

    protected override void PrepareDefense(DefenseDescription defense)
    {
      defense.Set(ObjectDefensePresets.Tier3);
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          .AddShapeRectangle((0.9, 0.5), offset: (0.1, 0.05))
          .AddShapeRectangle((0.95, 0.4), offset: (0.025, 0.8), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle((0.9, 0.2), offset: (0.05, 0.85), group: CollisionGroups.HitboxRanged)
          .AddShapeRectangle((0.9, 1.25), offset: (0.1, 0.05), group: CollisionGroups.ClickArea);
    }
  }
}