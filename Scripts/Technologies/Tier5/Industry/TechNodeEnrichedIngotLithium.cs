using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Industry
{
  public class TechNodeEnrichedIngotLithium : TechNode<TechGroupIndustryT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeEnrichedIngotLithium>();

      config.SetRequiredNode<TechNodeEnrichedIngotSteel>();
    }
  }
}