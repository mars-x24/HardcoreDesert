using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction
{
  public class TechNodeMassDriver : TechNode<TechGroupConstructionT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectMassDriver>();

      config.SetRequiredNode<TechNodeLandClaimT5>();
    }
  }
}