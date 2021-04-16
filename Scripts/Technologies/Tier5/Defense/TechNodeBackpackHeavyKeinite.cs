namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Defense
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeBackpackHeavyKeinite : TechNode<TechGroupDefenseT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
              .AddRecipe<RecipeBackpackHeavyKeinite>();

      config.SetRequiredNode<TechNodeSuperHeavySuit>();
    }
  }
}