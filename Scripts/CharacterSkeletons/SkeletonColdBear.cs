﻿using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.GameApi.Data.Physics;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
  public class SkeletonColdBear : ProtoCharacterSkeletonAnimal
  {
    public override double DefaultMoveSpeed => 1.0;

    public override Vector2D IconOffset => (0, -5);

    public override double IconScale => 0.5;

    public override SkeletonResource SkeletonResourceBack { get; }
        = new("ColdBear/Back");

    public override SkeletonResource SkeletonResourceFront { get; }
        = new("ColdBear/Front");

    public override double WorldScale => 0.65;

    protected override string SoundsFolderPath => "Skeletons/Bear";

    protected override double VolumeFootsteps => 1.0;

    public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
    {
      shadowRenderer.PositionOffset = (0, -0.2 * scaleMultiplier);
      shadowRenderer.Scale = 1.5 * scaleMultiplier;
    }

    public override void CreatePhysics(IPhysicsBody physicsBody)
    {
      physicsBody
          .AddShapeCircle(radius: 0.5,
                          center: (0, 0))
          .AddShapeCircle(radius: 0.65,
                          center: (0, 0.35),
                          group: CollisionGroups.HitboxMelee)
          .AddShapeCircle(radius: 0.65,
                          center: (0, 0.35),
                          group: CollisionGroups.HitboxRanged);
    }
  }
}