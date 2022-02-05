namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Teleport
{
  using AtomicTorch.CBND.CoreMod.Technologies.Tier5.Chemistry;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier5.Industry;

  public class TechGroupTeleportT5 : TechGroup
  {
    public override string Description => "Attempt to master the teleportation network";

    public override bool IsPrimary => false;

    public override string Name => "Teleport";

    public override TechTier Tier => TechTier.Tier5;

    protected override void PrepareTechGroup(Requirements requirements)
    {
      requirements.AddGroup<TechGroupConstructionT5>(completion: 1);
      requirements.AddGroup<TechGroupChemistryT5>(completion: 1);
      requirements.AddGroup<TechGroupIndustryT5>(completion: 1);
    }
  }
}