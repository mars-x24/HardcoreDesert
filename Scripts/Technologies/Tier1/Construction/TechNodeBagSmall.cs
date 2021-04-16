namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeBagSmall : TechNode<TechGroupConstructionT1>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
              .AddRecipe<RecipeBagSmall>();

      config.SetRequiredNode<TechNodeLandClaimT1>();
    }
  }
}