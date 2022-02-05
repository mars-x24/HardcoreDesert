namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Industry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;

  public class TechGroupIndustryT5 : TechGroup
    {
        public override string Description => TechGroupsLocalization.IndustryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.IndustryName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT4>(completion: 1);
        }
    }
}