using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking
{
  public class TechNodeButter : TechNode<TechGroupCookingT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeButter>();

      config.SetRequiredNode<TechNodeMilk>();
    }
  }
}