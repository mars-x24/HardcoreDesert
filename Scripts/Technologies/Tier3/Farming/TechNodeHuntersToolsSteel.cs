using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming
{
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