﻿using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
using AtomicTorch.CBND.CoreMod.Objects;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;
using AtomicTorch.GameEngine.Common.Primitives;
using System;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Environment
{
  public abstract class ProtoObjectIceCrack : ProtoObjectMisc, IProtoObjectColdSource
  {
    private Vector2D textureAltasAnimationDrawPositionWorldOffset;

    private ITextureAtlasResource textureAtlasAnimation;

    private double textureAtlasAnimationFrameDurationSeconds;

    public override bool CanFlipSprite => false;

    // necessary as ice crack is a light source
    public override bool HasIncreasedScopeSize => true;

    public virtual double ColdIntensity => 1;

    public abstract double ColdRadiusMax { get; }

    public abstract double ColdRadiusMin { get; }

    // define this object as a structure to prevent terrain decals rendered under it
    public override StaticObjectKind Kind => StaticObjectKind.Structure;

    public override string Name => "Fissure";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      var renderer = data.ClientState.Renderer;
      renderer.DrawOrder = DrawOrder.Floor;

      var overlayRenderer = Client.Rendering.CreateSpriteRenderer(
          data.GameObject,
          TextureResource.NoTexture);

      this.ClientSetupRenderer(overlayRenderer);

      overlayRenderer.PositionOffset = renderer.PositionOffset
                                       + this.textureAltasAnimationDrawPositionWorldOffset;
      overlayRenderer.SpritePivotPoint = renderer.SpritePivotPoint;
      overlayRenderer.Scale = renderer.Scale;
      overlayRenderer.DrawOrder = renderer.DrawOrder + 1;

      data.GameObject
          .ClientSceneObject
          .AddComponent<ClientComponentSpriteSheetBlendAnimator>()
          .Setup(
              overlayRenderer,
              ClientComponentSpriteSheetAnimator.CreateAnimationFrames(this.textureAtlasAnimation),
              frameDurationSeconds: this.textureAtlasAnimationFrameDurationSeconds);
    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      this.textureAtlasAnimation =
          this.PrepareTextureAtlasAnimation(out var animationFrameDurationSeconds,
                                            out var animationDrawPositionWorldOffset);
      this.textureAtlasAnimationFrameDurationSeconds = animationFrameDurationSeconds;
      this.textureAltasAnimationDrawPositionWorldOffset = animationDrawPositionWorldOffset;

      return base.PrepareDefaultTexture(thisType);
    }

    protected abstract ITextureAtlasResource PrepareTextureAtlasAnimation(
        out double frameDurationSeconds,
        out Vector2D drawPositionWorldOffset);
  }
}