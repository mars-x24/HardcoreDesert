namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.EnergyWeapons
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeStunPistol : TechNode<TechGroupEnergyWeaponsT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeStunPistol>();

      //config.SetRequiredNode<TechNodeLaserWeapons>(); // We will add special components for this weapons later
    }
  }
}