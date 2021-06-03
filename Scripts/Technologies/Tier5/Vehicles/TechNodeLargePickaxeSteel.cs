namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;
  using AtomicTorch.CBND.CoreMod.Vehicles;

  public class TechNodeLargePickaxeSteel : TechNode<TechGroupVehiclesT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeLargePickaxeSteel>();


      config.SetRequiredNode<TechNodeCrusher>();
    }
  }
}