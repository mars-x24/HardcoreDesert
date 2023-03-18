using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction
{
  public class TechNodeHugeGroundedCrate : TechNode<TechGroupConstructionT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectCrateHugeGrounded>();

      config.SetRequiredNode<TechNodeLandClaimT4>();
    }
  }
}