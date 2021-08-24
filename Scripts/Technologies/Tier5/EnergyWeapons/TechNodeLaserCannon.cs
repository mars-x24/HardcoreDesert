namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.EnergyWeapons
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeLaserCannon : TechNode<TechGroupEnergyWeaponsT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeLaserCannon>();

      config.SetRequiredNode<TechNodeStunPistol>(); // We will add special components for this weapons later
    }
  }
}