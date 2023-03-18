using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cooking
{
  public class TechNodeCornBreadTaco : TechNode<TechGroupCookingT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeCornBreadTaco>();

      config.SetRequiredNode<TechNodeSushi>();
    }
  }
}