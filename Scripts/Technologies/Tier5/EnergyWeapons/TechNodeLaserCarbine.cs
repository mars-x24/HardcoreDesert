namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.EnergyWeapons
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeLaserCarbine : TechNode<TechGroupEnergyWeaponsT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeLaserCarbine>();

      config.SetRequiredNode<TechNodeStunPistol>(); // We will add special components for this weapons later
    }
  }
}