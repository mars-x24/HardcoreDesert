using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry
{
  public class TechNodeRobotLogisticAdvanced : TechNode<TechGroupIndustryT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeRobotLogisticAdvanced>();

      config.SetRequiredNode<TechNodeComponentsHighTech>();
    }
  }
}