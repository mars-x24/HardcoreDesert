using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
  public class TechNodeProjectorWall : TechNode<TechGroupElectricityT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectLightProjectorWall>();

      config.SetRequiredNode<TechNodeGeneratorPragmiumReactor>();
    }
  }
}