using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Fishing
{
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