namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeBagLarge : TechNode<TechGroupConstructionT2> 
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
              .AddRecipe<RecipeBagLarge>();

      config.SetRequiredNode<TechNodeLandClaimT2>();
    }
  }
}