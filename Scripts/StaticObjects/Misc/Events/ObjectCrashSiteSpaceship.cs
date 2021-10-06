namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;
  using System.Linq;

  public class ObjectCrashSiteSpaceship : ProtoStaticWorldObject
  {
    public override string Name => "Crashed Spaceship";

    public override StaticObjectKind Kind => StaticObjectKind.Structure;

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override double ObstacleBlockDamageCoef => 1.0;

    public override float StructurePointsMax => 10000;

    public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
    {
      return (this.Layout.Center.X, 0.8);
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.PositionOffset = (0 / 256.0, 0 / 256.0);
      renderer.DrawOrderOffsetY = 3.55;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("     ###",
                   "     ###",
                   "########",
                   "########",
                   "########",
                   "   ###",
                   "   ###");
    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return new TextureResource("StaticObjects/Misc/Events/ObjectCrashSiteShip",
                                 isTransparent: true);
    }



    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      //if (data.IsFirstTimeInit
      //    && !data.GameObject.OccupiedTile.StaticObjects
      //            .Any(o => o.ProtoGameObject is ObjectCrashSiteSpaceshipGround))
      //{
      //  Server.World.CreateStaticWorldObject<ObjectCrashSiteSpaceshipGround>(data.GameObject.TilePosition);
      //}
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          .AddShapeCircle(radius: 0.45, center: (1.35, 3.35)) //nose
          .AddShapeCircle(radius: 0.55, center: (1.6, 3.3)) //nose
          .AddShapeCircle(radius: 0.7, center: (2.4, 3.4)) //cockpit
          .AddShapeCircle(radius: 1.0, center: (4.15, 2.7)) //botton wing - body
          .AddShapeCircle(radius: 0.9, center: (4.3, 1.8)) //bottom wing - mid segment
          .AddShapeCircle(radius: 0.8, center: (4.8, 2.5)) //bottom wing - turbine
          .AddShapeCircle(radius: 1.0, center: (5.2, 3.8)) //body
          .AddShapeCircle(radius: 1.2, center: (6.2, 3.5)) //body - rear
          .AddShapeCircle(radius: 0.95, center: (6.0, 4.55)) //upper wing - tip

          .AddShapeRectangle(size: (2.0, 1.4), offset: (2.5, 2.7)) //body
          .AddShapeRectangle(size: (1.6, 1.3), offset: (5.8, 3.2)) //rear
          .AddShapeRectangle(size: (1.1, 1.0), offset: (5.8, 4.5)) //upper wing - tip
          .AddShapeRectangle(size: (1.4, 0.5), offset: (3.6, 1.0)) //bottom wing - tip


          .AddShapeRectangle(size: (1.75, 1.0), offset: (2.0, 3.0), group: CollisionGroups.HitboxRanged)
          .AddShapeRectangle(size: (1.25, 2.0), offset: (3.75, 2.6), group: CollisionGroups.HitboxRanged)
          .AddShapeRectangle(size: (1.0, 2.0), offset: (5.0, 3.0), group: CollisionGroups.HitboxRanged)


          .AddShapeRectangle(size: (1.75, 0.75), offset: (2.0, 3.0), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (1.25, 2.0), offset: (3.75, 2.6), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (1.0, 2.0), offset: (5.0, 3.0), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (1.4, 1.3), offset: (6.0, 3.2), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.8, 1.0), offset: (6.0, 4.5), group: CollisionGroups.HitboxMelee);
    }
  }
}