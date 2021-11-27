namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Electricity;

  public class TechNodeWireFromRubberRaw : TechNode<TechGroupElectricityT1>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeWireFromRubberRaw>();
    }
  }
}