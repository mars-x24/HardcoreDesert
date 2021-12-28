namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeHuntersToolsSteel : TechNode<TechGroupFarmingT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeHuntersToolsSteel>();

      config.SetRequiredNode<TechNodeSpices>();
    }
  }
}