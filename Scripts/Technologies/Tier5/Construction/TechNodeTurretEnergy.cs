using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction
{
  public class TechNodeTurretEnergy : TechNode<TechGroupConstructionT5>
  {
    public override FeatureAvailability AvailableIn => FeatureAvailability.All;

    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectTurretEnergy>();

      config.SetRequiredNode<TechNodeSteelConstructions>();
    }
  }
}