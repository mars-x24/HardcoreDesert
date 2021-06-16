namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;

  public class TechNodeTurretHeavy : TechNode<TechGroupConstructionT4>
  {
    public override FeatureAvailability AvailableIn => FeatureAvailability.All;

    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectTurretHeavy>();

      config.SetRequiredNode<TechNodeConcreteConstructions>();
    }
  }
}