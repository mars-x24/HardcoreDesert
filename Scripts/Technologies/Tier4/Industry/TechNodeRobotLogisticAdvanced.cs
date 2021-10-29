namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

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