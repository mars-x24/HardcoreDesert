using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry
{
  public class TechNodeSwordSteel : TechNode<TechGroupIndustryT2>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeSwordSteel>();

      config.SetRequiredNode<TechNodeSteelTools>();
    }
  }
}