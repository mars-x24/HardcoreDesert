using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Teleport
{
  public class TechNodeTeleportAlien4 : TechNode<TechGroupTeleportT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeTeleportAlien4>();

      config.SetRequiredNode<TechNodeTeleportAlien1>();
    }
  }
}