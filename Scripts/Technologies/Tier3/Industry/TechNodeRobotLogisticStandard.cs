namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeRobotLogisticStandard : TechNode<TechGroupIndustryT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeRobotLogisticStandard>();

      config.SetRequiredNode<TechNodeComponentsElectronic>();
    }
  }
}