using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
  public class TechNodeKatana : TechNode<TechGroupIndustryT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeKatana>();

      config.SetRequiredNode<TechNodeOilRefinery>();
    }
  }
}