using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity
{
  public class TechNodeBagFreezer : TechNode<TechGroupElectricityT4>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeBagFreezer>();

      config.SetRequiredNode<TechNodeFridgeFreezer>();
    }
  }
}