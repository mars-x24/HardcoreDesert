using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Electricity;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
  public class TechNodeGeneratorBio : TechNode<TechGroupElectricityT1>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectGeneratorBio>();

      config.SetRequiredNode<TechNodeWireFromRubberRaw>();
    }
  }
}