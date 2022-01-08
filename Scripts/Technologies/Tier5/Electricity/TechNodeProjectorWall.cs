namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

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