namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Chemistry
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeEnrichedPlastic : TechNode<TechGroupChemistryT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeEnrichedPlastic>();
    }
  }
}