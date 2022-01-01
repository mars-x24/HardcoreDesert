namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Fishing
{
  using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Fishing;

  public class TechGroupFishingT3 : TechGroup
  {
    public override string Description => TechGroupsLocalization.FishingDescription;

    public override string Name => TechGroupsLocalization.FishingName;

    public override TechTier Tier => TechTier.Tier3;

    protected override void PrepareTechGroup(Requirements requirements)
    {
      requirements.AddGroup<TechGroupFishingT2>(completion: 1.0);
    }
  }
}