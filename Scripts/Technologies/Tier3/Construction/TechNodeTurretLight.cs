using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
  public class TechNodeTurretLight : TechNode<TechGroupConstructionT3>
  {
    public override FeatureAvailability AvailableIn => FeatureAvailability.All;

    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectTurretLight>();

      config.SetRequiredNode<TechNodeWallFence>();
    }
  }
}