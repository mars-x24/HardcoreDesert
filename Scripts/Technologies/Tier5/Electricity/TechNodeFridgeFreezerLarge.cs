namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

  public class TechNodeFridgeFreezerLarge : TechNode<TechGroupElectricityT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectFridgeFreezerLarge>();

      config.SetRequiredNode<TechNodeGeneratorPragmiumReactor>();
    }
  }
}