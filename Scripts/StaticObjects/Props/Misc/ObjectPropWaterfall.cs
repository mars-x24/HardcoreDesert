namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
  using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using System;
  using System.Linq;

  public class ObjectPropWaterfall : ProtoObjectProp
  {
    private static readonly SoundResource SoundResourceActive
    = new("Objects/Waterfall/Active");

    private TextureAtlasResource textureAtlas;

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("####",
                   "####",
				   "####");
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
        data.PhysicsBody
            .AddShapeRectangle(size: (2.0, 3.0),
                               offset: (1, 0));
    }

    public override StaticObjectKind Kind => StaticObjectKind.Platform;

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrder = DrawOrder.Floor - 1;
    }

    protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
    {
      tileRequirements.Clear()
                      .Add("No other platforms allowed",
                           c => c.Tile.StaticObjects.All(
                               o => o.ProtoStaticWorldObject.Kind != StaticObjectKind.Platform));
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      // setup animation
      var animationFrameDurationSeconds = 1 / 8.0;
      var clientState = data.ClientState;

      data.GameObject
          .ClientSceneObject
          .AddComponent<ClientComponentSpriteSheetAnimator>()
          .Setup(
              clientState.Renderer,
              ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlas),
              isLooped: true,
              frameDurationSeconds: animationFrameDurationSeconds,
              randomizeInitialFrame: true);

      // create sound
      clientState.SoundEmitter = Client.Audio.CreateSoundEmitter(
          data.GameObject,
          SoundResourceActive,
          isLooped: true,
          volume: 0.5f,
          radius: 2.5f);
      clientState.SoundEmitter.CustomMaxDistance = 8;
    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      var texturePath = GenerateTexturePath(typeof(ObjectPropWaterfall));
      this.textureAtlas = new TextureAtlasResource(
          texturePath,
          columns: 5,
          rows: 1,
          isTransparent: true);

      return this.textureAtlas;
    }
  }
}