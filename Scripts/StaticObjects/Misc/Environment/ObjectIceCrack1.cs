﻿using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Environment
{
  public class ObjectIceCrack1 : ProtoObjectIceCrack
  {
    public override double ColdRadiusMax => 6;

    public override double ColdRadiusMin => 3;

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      ClientLighting.CreateLightSourceSpot(
          data.GameObject.ClientSceneObject,
          color: LightColors.Ice,
          size: (10.5, 15),
          positionOffset: (1, 1.5));
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrderOffsetY = 3.0;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("##",
                   "##",
                   "##");
    }

    protected override ITextureAtlasResource PrepareTextureAtlasAnimation(
        out double frameDurationSeconds,
        out Vector2D drawPositionWorldOffset)
    {
      frameDurationSeconds = 1.8;
      drawPositionWorldOffset = (53 / 256.0, 148 / 256.0);
      return new TextureAtlasResource(this.GenerateTexturePath() + "Animation",
                                      columns: 3,
                                      rows: 1,
                                      isTransparent: true);
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      /*data.PhysicsBody
          .AddShapeCircle(radius: 0.4, center: (0.55, 1.0))
          .AddShapeCircle(radius: 0.4, center: (0.8, 1.5))
          .AddShapeCircle(radius: 0.4, center: (1.35, 2.05));*/
    }
  }
}