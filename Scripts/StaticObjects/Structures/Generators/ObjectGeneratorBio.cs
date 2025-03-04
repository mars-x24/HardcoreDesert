﻿using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.Systems.Construction;
using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
  [ElectricityProductionOrder(afterType: typeof(ObjectGeneratorSolar))]
  public class ObjectGeneratorBio : ProtoObjectGeneratorBio
  {
    public override byte ContainerInputSlotsCount => 4;

    public override ElectricityThresholdsPreset DefaultGenerationElectricityThresholds
        => new(startupPercent: 99,
               shutdownPercent: 100);

    public override string Description =>
        "Decomposes organic matter into methane gas, which is used to produce small amounts of electrical energy. Not suitable as the primary source of electricity.";

    public override string Name => "Bioreactor";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override double ObstacleBlockDamageCoef => 1;

    public override ushort OrganicCapacity => 2000; //MOD

    public override double OrganicDecreasePerSecondWhenActive => 0.05;

    public override float StructurePointsMax => 3000;

    public override void SharedGetElectricityProduction(
        IStaticWorldObject worldObject,
        out double currentProduction,
        out double maxProduction)
    {
      maxProduction = 3;

      var publicState = GetPublicState(worldObject);
      currentProduction = publicState.IsActive
                              ? maxProduction
                              : 0;
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      var worldObject = data.GameObject;
      var publicState = data.PublicState;

      var soundEmitter = Client.Audio.CreateSoundEmitter(
          worldObject,
          soundResource: worldObject.ProtoStaticWorldObject
                                    .SharedGetObjectSoundPreset()
                                    .GetSound(ObjectSound.Active),
          is3D: true,
          radius: Client.Audio.CalculateObjectSoundRadius(worldObject),
          isLooped: true);
      soundEmitter.Volume = 0.4f;
      soundEmitter.Radius = 2f;
      soundEmitter.CustomMaxDistance = 6f;

      publicState.ClientSubscribe(_ => _.IsActive,
                                  _ => RefreshActiveState(),
                                  data.ClientState);

      RefreshActiveState();

      void RefreshActiveState()
      {
        soundEmitter.IsEnabled = publicState.IsActive;
      }
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.PositionOffset += (0, 0.1);
      renderer.DrawOrderOffsetY = 0.7;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("##",
                   "##");
    }

    protected override void PrepareConstructionConfig(
        ConstructionTileRequirements tileRequirements,
        ConstructionStageConfig build,
        ConstructionStageConfig repair,
        ConstructionUpgradeConfig upgrade,
        out ProtoStructureCategory category)
    {
      tileRequirements.Add(LandClaimSystem.ValidatorIsOwnedLand);
      category = GetCategory<StructureCategoryElectricity>();

      build.StagesCount = 10;
      build.StageDurationSeconds = BuildDuration.Medium;

      build.AddStageRequiredItem<ItemWire>(count: 4);
      build.AddStageRequiredItem<ItemGlassRaw>(count: 2);
      build.AddStageRequiredItem<ItemIngotIron>(count: 1);
      build.AddStageRequiredItem<ItemCharcoal>(count: 1);
      build.AddStageRequiredItem<ItemSand>(count: 2);
      build.AddStageRequiredItem<ItemPotassiumNitrate>(count: 1);

      repair.StagesCount = 10;
      repair.StageDurationSeconds = BuildDuration.Medium;
      repair.AddStageRequiredItem<ItemWire>(count: 3);
      repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
      repair.AddStageRequiredItem<ItemCharcoal>(count: 1);
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          .AddShapeRectangle(size: (1.5, 0.95), offset: (0.25, 0.35))
          .AddShapeRectangle(size: (1.2, 1.6), offset: (0.4, 0.3), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (1.2, 0.35), offset: (0.4, 1.3), group: CollisionGroups.HitboxRanged)
          .AddShapeRectangle(size: (1.2, 1.6), offset: (0.4, 0.3), group: CollisionGroups.ClickArea);
    }
  }
}