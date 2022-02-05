namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Alien
{
  using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
  using AtomicTorch.CBND.CoreMod.Helpers.Client;
  using AtomicTorch.CBND.CoreMod.Items;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;

  public class ObjectPropAlienTeleportBigBoss : ProtoObjectProp
  {
    public override bool HasIncreasedScopeSize => true;

    private static ushort scale = 4;

    private static readonly Vector2Ushort ComposedTextureSize = (1024, 1076);

    private static readonly IReadOnlyItemLightConfig LightSourceConfig
        = new ItemLightConfig()
        {
          Color = LightColors.Alien,
          LogicalSize = (15 * scale, 15 * scale),
          Size = (15 * scale, 15 * scale),
          WorldOffset = (522.0 * scale / ScriptingConstants.TileSizeRealPixels,
                           824.0 * scale / ScriptingConstants.TileSizeRealPixels)
        };

    private static readonly Vector2Ushort TextureResourceBackOffset = (106, 486);

    private static readonly Vector2Ushort TextureResourceBackSize = (813, 590);

    private static readonly Vector2Ushort TextureResourceBaseSize = (1024, 768);

    private static readonly Vector2Ushort TextureResourceFrontOffset = (106, 82);

    private static readonly Vector2Ushort TextureResourceFrontSize = (813, 751);

    private readonly ITextureResource textureResourceBack;

    private readonly TextureResource textureResourceBase;

    private readonly ITextureResource textureResourceFront;

    public ObjectPropAlienTeleportBigBoss()
    {
      var texturePath = $"StaticObjects/Props/Alien/{nameof(ObjectPropAlienTeleportBigBoss)}";
      this.textureResourceBack = new TextureResource(texturePath + "Back");
      this.textureResourceBase = new TextureResource(texturePath + "Base");
      this.textureResourceFront = new TextureResource(texturePath + "Front");
    }

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
    {
      var renderer = blueprint.SpriteRenderer;
      renderer.TextureResource = this.Icon;
      renderer.Size = (ComposedTextureSize.X * 0.5,
                       ComposedTextureSize.Y * 0.5);
    }

    public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
    {
      return base.SharedGetObjectCenterWorldOffset(worldObject) + (0, 1.25);
    }

    protected override ITextureResource ClientCreateIcon()
    {
      return ClientProceduralTextureHelper.CreateComposedTexture(
          "Composed " + this.Id,
          isTransparent: true,
          isUseCache: true,
          customSize: ComposedTextureSize,
          textureResourcesWithOffsets: new[]
          {
                    new TextureResourceWithOffset(this.textureResourceBase,
                                                  offset: (0, TextureResourceBaseSize.Y - ComposedTextureSize.Y)),
                    new TextureResourceWithOffset(this.textureResourceBack,
                                                  offset: (0, TextureResourceBackSize.Y - ComposedTextureSize.Y)
                                                          + TextureResourceBackOffset.ToVector2D()),
                    new TextureResourceWithOffset(this.textureResourceFront,
                                                  offset: (0, TextureResourceFrontSize.Y - ComposedTextureSize.Y)
                                                          + TextureResourceFrontOffset.ToVector2D())
          });
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      Client.Rendering.CreateSpriteRenderer(
          data.GameObject,
          this.textureResourceBack,
          scale: scale,
          positionOffset: TextureResourceBackOffset.ToVector2D() * scale
                          / ScriptingConstants.TileSizeRealPixels);

      Client.Rendering.CreateSpriteRenderer(
          data.GameObject,
          this.textureResourceFront,
          scale: scale,
          positionOffset: TextureResourceFrontOffset.ToVector2D() * scale
                          / ScriptingConstants.TileSizeRealPixels);

      ClientLighting.CreateLightSourceSpot(data.GameObject.ClientSceneObject,
                                           LightSourceConfig);
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrder = DrawOrder.Floor + 1;
      renderer.Scale = scale;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################",
                   "################");
    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return this.textureResourceBase;
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          // bl
          .AddShapeRectangle(size: (0.8 * scale, 0.4 * (scale + 1)), offset: (0.4 * scale, 0.33 * scale))
          .AddShapeRectangle(size: (0.8 * scale, 0.5 * scale), offset: (0.4 * scale, 0.5 * scale), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6 * scale, 0.4 * scale), offset: (0.5 * scale, 1.1 * scale), group: CollisionGroups.HitboxRanged)
          // br
          .AddShapeRectangle(size: (0.8 * scale, 0.4 * (scale + 1)), offset: (2.8 * scale, 0.33 * scale))
          .AddShapeRectangle(size: (0.8 * scale, 0.5 * scale), offset: (2.8 * scale, 0.5 * scale), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6 * scale, 0.4 * scale), offset: (2.9 * scale, 1.1 * scale), group: CollisionGroups.HitboxRanged)
          // tl
          .AddShapeRectangle(size: (0.8 * scale, 0.4 * (scale + 1)), offset: (0.4 * scale, 1.9 * scale))
          .AddShapeRectangle(size: (0.8 * scale, 0.5 * scale), offset: (0.4 * scale, 2.0 * scale), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6 * scale, 0.4 * scale), offset: (0.5 * scale, 2.6 * scale), group: CollisionGroups.HitboxRanged)
          // tr
          .AddShapeRectangle(size: (0.8 * scale, 0.4 * (scale + 1)), offset: (2.8 * scale, 1.9 * scale))
          .AddShapeRectangle(size: (0.8 * scale, 0.5 * scale), offset: (2.8 * scale, 2.0 * scale), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6 * scale, 0.4 * scale), offset: (2.9 * scale, 2.6 * scale), group: CollisionGroups.HitboxRanged)
          // click
          .AddShapeRectangle(size: (3.6 * scale, 2.2 * scale), offset: (0.2 * scale, 0.15 * scale), group: CollisionGroups.ClickArea);
    }
  }
}