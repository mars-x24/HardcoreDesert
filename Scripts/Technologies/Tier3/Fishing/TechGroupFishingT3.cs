namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Fishing
{
  using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;

  public class TechGroupFishingT3 : TechGroup
  {
    public override string Description => TechGroupsLocalization.FishingDescription;

    public override string Name => TechGroupsLocalization.FishingName;

    public override TechTier Tier => TechTier.Tier3;

    protected override void PrepareTechGroup(Requirements requirements)
    {
      requirements.AddGroup<TechGroupIndustryT2>(completion: 0.6);
      requirements.AddGroup<TechGroupConstructionT2>(completion: 0.6);
      requirements.AddGroup<TechGroupChemistryT3>(completion: 0.2);
    }
  }
}