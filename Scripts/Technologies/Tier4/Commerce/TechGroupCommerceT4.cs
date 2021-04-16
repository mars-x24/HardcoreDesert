namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Commerce
{
  using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Commerce;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity;

  public class TechGroupCommerceT4 : TechGroup
  {
    public override string Description => TechGroupsLocalization.CommerceDescription;

    public override string Name => TechGroupsLocalization.CommerceName;

    public override TechTier Tier => TechTier.Tier4;

    protected override void PrepareTechGroup(Requirements requirements)
    {
      requirements.AddGroup<TechGroupIndustryT3>(completion: 0.8);
      requirements.AddGroup<TechGroupConstructionT3>(completion: 0.8);
      requirements.AddGroup<TechGroupCommerceT3>(completion: 1.0);
      requirements.AddGroup<TechGroupElectricityT4>(completion: 1.0);
    }
  }
}