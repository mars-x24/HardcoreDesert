using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
using AtomicTorch.CBND.CoreMod.Systems.Construction;
using AtomicTorch.CBND.CoreMod.Systems.Notifications;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
  public class ObjectMassDriver : ProtoObjectGlobalChest
  {
    public override string Description =>
        "You can move any object between mass driver crate.";

    public override bool HasOwnersList => false;

    public override byte ItemsSlotsCount => 64;

    public override string Name => "Ender crate";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override double ObstacleBlockDamageCoef => 0.5;

    public override double StructureExplosiveDefenseCoef => 0.25;

    public override float StructurePointsMax => 1500;

    public override bool IsSupportItemIcon => false;

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrderOffsetY = 0.3;
    }

    protected override BaseUserControlWithWindow ClientOpenUI(IStaticWorldObject worldObject, ObjectCratePrivateState privateState)
    {
      var publicState = worldObject.GetPublicState<ObjectGlobalChestPublicState>();
      if (publicState.LandClaimGroup is null || privateState.ItemsContainer is null)
      {
        NotificationSystem.ClientShowNotification("Ender crate", "Land claim needed to update locations.", NotificationColor.Bad);
        return null;
      }

      return WindowMassDriverContainer.Show(worldObject, privateState);
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
      category = GetCategory<StructureCategoryStorage>();

      build.StagesCount = 5;
      build.StageDurationSeconds = BuildDuration.Long;
      build.AddStageRequiredItem<ItemIngotSteel>(count: 10);
      build.AddStageRequiredItem<ItemCement>(count: 5);
      build.AddStageRequiredItem<ItemComponentsMechanical>(count: 2);
      build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
      build.AddStageRequiredItem<ItemOrePragmium>(count: 10);

      repair.StagesCount = 5;
      repair.StageDurationSeconds = BuildDuration.Long;
      repair.AddStageRequiredItem<ItemIngotSteel>(count: 5);
      repair.AddStageRequiredItem<ItemComponentsMechanical>(count: 1);
      repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
      repair.AddStageRequiredItem<ItemOrePragmium>(count: 2);
    }

    protected override void PrepareDefense(DefenseDescription defense)
    {
      defense.Set(ObjectDefensePresets.Tier4);
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          .AddShapeRectangle(size: (2.4, 1.1), offset: (-0.2, 0.4))
          .AddShapeRectangle(size: (1.4, 0.9), offset: (0.3, 0.4), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (1.4, 0.9), offset: (0.3, 0.4), group: CollisionGroups.ClickArea);
    }

  }
}
