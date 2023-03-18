using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
  public class TechNodeCornBread : TechNode<TechGroupCookingT2>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeCornBread>();

      config.SetRequiredNode<TechNodeCornflourDough>();
    }
  }
}