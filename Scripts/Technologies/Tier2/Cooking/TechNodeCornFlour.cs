using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
  public class TechNodeCornFlour : TechNode<TechGroupCookingT2>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeCornFlour>();

      config.SetRequiredNode<TechNodeInsectMeatFried>();
    }
  }
}