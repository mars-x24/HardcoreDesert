namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Fishing
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

  public class TechNodeFishingPragmiumBaitMix : TechNode<TechGroupFishingT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeFishingPragmiumBaitMix>();

      config.SetRequiredNode<TechNodeFishingRodPragmium>();
    }
  }
}