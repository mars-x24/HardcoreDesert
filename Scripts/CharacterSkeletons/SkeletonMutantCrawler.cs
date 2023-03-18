using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.GameApi.Data.Physics;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;

namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
  public class SkeletonMutantCrawler : ProtoCharacterSkeletonAnimal
  {
    public override double DefaultMoveSpeed => 2.5;

    public override double IconScale => 0.8;

    public override SkeletonResource SkeletonResourceBack { get; }
        = new("MutantCrawler/Back");

    public override SkeletonResource SkeletonResourceFront { get; }
        = new("MutantCrawler/Front");

    public override double WorldScale => 0.6;

    protected override string SoundsFolderPath => "Skeletons/Crawler";

    public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
    {
      shadowRenderer.PositionOffset = (0, -0.1 * scaleMultiplier);
      shadowRenderer.Scale = 0.6 * scaleMultiplier;
    }

    public override void CreatePhysics(IPhysicsBody physicsBody)
    {
      physicsBody
          .AddShapeCircle(radius: 0.3,
                          center: (0, 0.125))
          .AddShapeCircle(radius: 0.35,
                          center: (0, 0.25),
                          group: CollisionGroups.HitboxMelee)
          .AddShapeCircle(radius: 0.35,
                          center: (0, 0.25),
                          group: CollisionGroups.HitboxRanged);
    }
  }
}