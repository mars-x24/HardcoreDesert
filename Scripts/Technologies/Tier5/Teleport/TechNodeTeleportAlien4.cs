namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Teleport
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;

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