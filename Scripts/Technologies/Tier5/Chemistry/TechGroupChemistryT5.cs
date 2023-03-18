using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Chemistry
{
  public class TechGroupChemistryT5 : TechGroup
  {
    public override string Description => TechGroupsLocalization.ChemistryDescription;

    public override bool IsPrimary => true;

    public override string Name => TechGroupsLocalization.ChemistryName;

    public override TechTier Tier => TechTier.Tier5;

    protected override void PrepareTechGroup(Requirements requirements)
    {
      requirements.AddGroup<TechGroupChemistryT4>(completion: 1);
    }
  }
}