using AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.EnergyWeapons
{
  public class TechGroupEnergyWeaponsT5 : TechGroup
  {
    public override string Description => TechGroupsLocalization.EnergyWeaponsDescription;

    public override string Name => TechGroupsLocalization.EnergyWeaponsName;

    public override TechTier Tier => TechTier.Tier5;

    protected override void PrepareTechGroup(Requirements requirements)
    {
      requirements.AddGroup<TechGroupEnergyWeaponsT4>(completion: 1);
    }
  }
}