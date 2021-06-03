namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;
  using AtomicTorch.CBND.CoreMod.Vehicles;

  public class TechNodeLargeAxeSteel : TechNode<TechGroupVehiclesT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeLargeAxeSteel>();


      config.SetRequiredNode<TechNodeCrusher>();
    }
  }
}