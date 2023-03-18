using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.GameEngine.Common.Primitives;
using System.Linq;
using System.Windows.Media;

namespace AtomicTorch.CBND.CoreMod.Vehicles
{
  public class VehicleHovercraftMk1 : ProtoVehicleHoverboard, IProtoVehicleLootOnWater
  {
    public override string Description =>
        "Heavy variant of gravi-platform design capable of moving over water or land.";

    public override ushort EnergyUsePerSecondIdle => 10;

    public override ushort EnergyUsePerSecondMoving => 70;

    public override Color LightColor => LightColors.Flashlight.WithAlpha(0x88);

    public override Size2F LightLogicalSize => 14;

    public override Vector2D LightPositionOffset => (0, -0.25);

    public override Size2F LightSize => 6;

    public override string Name => "Hovercraft Mk1";

    public override double PhysicsBodyAccelerationCoef => 2;

    public override double PhysicsBodyFriction => 6;

    public override double StatMoveSpeed => 4.2;

    public override float StructurePointsMax => 400;

    public override TextureResource TextureResourceHoverboard { get; }
        = new("Vehicles/HovercraftMk1");

    public override TextureResource TextureResourceHoverboardLight { get; }
        = new("Vehicles/HovercraftMk1Light");

    public override bool VehicleBarEnabled => true;

    public override double VehicleWorldHeight => 0.5;

    protected override SoundResource EngineSoundResource { get; }
        = new("Objects/Vehicles/Hoverboard/Engine2");

    protected override double EngineSoundVolume => 0.5;

    protected override bool IsPilotDismountedOnDamaged => false;

    public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
    {
      return (0, -0.3);
    }

    protected override void PrepareDefense(DefenseDescription defense)
    {
      defense.Set(ObjectDefensePresets.Tier2);
    }

    protected override void PrepareProtoVehicle(
        InputItems buildRequiredItems,
        InputItems repairStageRequiredItems,
        out int repairStagesCount)
    {
      buildRequiredItems
          .Add<ItemEnrichedIngotSteel>(60)
          .Add<ItemComponentsMechanical>(10)
          .Add<ItemImpulseEngine>(1)
          .Add<ItemComponentsHighTech>(8);

      repairStageRequiredItems
          .Add<ItemEnrichedIngotSteel>(8);

      repairStagesCount = 5;
    }

    public override void ServerOnPilotDamage(
        WeaponFinalCache weaponCache,
        IDynamicWorldObject vehicle,
        ICharacter pilotCharacter,
        double damageApplied)
    {
      if (damageApplied > 0)
      {
        // drop from hoverboard on any character damage
        //VehicleSystem.ServerCharacterExitCurrentVehicle(pilotCharacter, force: true);
      }
    }

    protected override void ServerUpdateVehicle(ServerUpdateData data)
    {
      base.ServerUpdateVehicle(data);

      var vehicle = data.GameObject;
      var publicState = data.PublicState;

      //MOD check if the character is over water
      if (vehicle.Tile.ProtoTile.Kind == TileKind.Water)// && !Api.IsEditor)
      {
        //No pilot
        if (publicState.PilotCharacter is null)
        {
          //No platform
          if (vehicle.Tile.StaticObjects.All(o => o.ProtoStaticWorldObject.Kind != StaticObjectKind.Platform))
          {
            //Move vehicle to shore
            if (!PlayerCharacter.ServerMovePlayerToShore(vehicle))
            {
              //Move vehicle to garage
              VehicleGarageSystem.ServerPutIntoGarage(vehicle);
            }
          }
        }
      }
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      var physicsBody = data.PhysicsBody;
      if (data.PublicState.PilotCharacter is null)
      {
        // no pilot
        physicsBody.AddShapeRectangle(size: (0.9, 0.6),
                                      offset: (-0.45, -0.4),
                                      group: CollisionGroups.ClickArea)
                   .AddShapeRectangle(size: (0.9, 0.6),
                                      offset: (-0.45, -0.4),
                                      group: CollisionGroups.HitboxMelee);
      }
      else
      {
        // like human legs collider but much larger
        var radius = 0.35;
        var colliderY = -0.115;

        // Please note: the top edge of the collider should match the top edge of standing player character,
        // otherwise player character's hitbox may go a bit outside the wall above when the hoverboard
        // is too close to it.
        AddShapes(offsetY: 0.04);
        AddShapes(offsetY: -0.1);

        void AddShapes(double offsetY)
        {
          physicsBody.AddShapeCircle(
              radius / 2,
              center: (-radius / 2, offsetY + colliderY),
              CollisionGroups.HoverWater);

          physicsBody.AddShapeCircle(
              radius / 2,
              center: (radius / 2, offsetY + colliderY),
              CollisionGroups.HoverWater);

          physicsBody.AddShapeRectangle(
              size: (radius, radius),
              offset: (-radius / 2, offsetY + colliderY - radius / 2),
              CollisionGroups.HoverWater);
        }
      }
    }
  }
}