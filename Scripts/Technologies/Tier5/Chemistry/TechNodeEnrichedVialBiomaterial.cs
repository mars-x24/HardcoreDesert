using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Chemistry
{
  public class TechNodeEnrichedVialBiomaterial : TechNode<TechGroupChemistryT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeEnrichedVialBiomaterial>();

      config.SetRequiredNode<TechNodeEnrichedPlastic>();
    }
  }
}