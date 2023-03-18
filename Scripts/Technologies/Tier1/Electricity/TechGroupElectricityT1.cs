namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Electricity
{
  public class TechGroupElectricityT1 : TechGroup
  {
    public override string Description => TechGroupsLocalization.ElectricityDescription;

    public override bool IsPrimary => true;

    public override string Name => TechGroupsLocalization.ElectricityName;

    public override TechTier Tier => TechTier.Tier1;

    protected override void PrepareTechGroup(Requirements requirements)
    {

    }
  }
}