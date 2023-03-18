using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense
{
  public class TechNodeBackpackMilitary : TechNode<TechGroupDefenseT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
              .AddRecipe<RecipeBackpackMilitary>();

      config.SetRequiredNode<TechNodeMilitaryArmor>();
    }
  }
}