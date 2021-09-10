namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.GameApi.Data.Physics;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;

  public class NPC_BA_Specialist : ProtoCharacterSkeletonNPC
  {
    public override double DefaultMoveSpeed => 2.0;

    public override double IconScale => 0.15;
    
    public override double WorldScale => 0.15;

    //From ProtoCharacterSkeletonHuman.

    public const double LegsColliderRadius = 0.2;

    public const double MeleeHitboxHeight = 0.7;

    public const double MeleeHitboxOffset = 0.25;

    public const double RangedHitboxHeight = 1.4;

    public const double RangedHitboxOffset = 0;

    //End ProtoCharacterSkeletonHuman.

    public override SkeletonResource SkeletonResourceBack { get; }
        = new SkeletonResource("NPC_BA_Specialist/MaleBack");

    public override SkeletonResource SkeletonResourceFront { get; }
        = new SkeletonResource("NPC_BA_Specialist/MaleFront");

    public override string SlotNameItemInHand => "Weapon";

    public override void ClientResetItemInHand(IComponentSkeleton skeletonRenderer)
    {
      skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand,
                                           attachmentName: "WeaponRifle",
                                           textureResource: null);
      skeletonRenderer.SetAttachment(this.SlotNameItemInHand, attachmentName: "WeaponRifle");
    }

    protected override string SoundsFolderPath => "Skeletons/Human";

    public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
    {
      shadowRenderer.PositionOffset = (0, -0.01 * scaleMultiplier);
      shadowRenderer.Scale = 1.5 * scaleMultiplier;
    }
    //From ProtoCharacterSkeletonHuman
    public override void CreatePhysics(IPhysicsBody physicsBody)
    {
      const double radius = LegsColliderRadius;
      physicsBody.AddShapeCircle(
          radius / 2,
          center: (-radius / 2, 0));

      physicsBody.AddShapeCircle(
          radius / 2,
          center: (radius / 2, 0));

      physicsBody.AddShapeRectangle(
          size: (radius, radius),
          offset: (-radius / 2, -radius / 2));

      // melee hitbox
      physicsBody.AddShapeRectangle(
          size: (0.6, MeleeHitboxHeight),
          offset: (-0.3, MeleeHitboxOffset),
          group: CollisionGroups.HitboxMelee);

      // ranged hitbox
      physicsBody.AddShapeRectangle(
          size: (0.5, RangedHitboxHeight),
          offset: (-0.25, RangedHitboxOffset),
          group: CollisionGroups.HitboxRanged);
      //End ProtoCharacterSkeletonHuman.
    }
  }
}