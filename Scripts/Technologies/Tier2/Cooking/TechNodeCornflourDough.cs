using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
  public class TechNodeCornflourDough : TechNode<TechGroupCookingT2>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeCornflourDough>();

      config.SetRequiredNode<TechNodeCornFlour>();
    }
  }
}