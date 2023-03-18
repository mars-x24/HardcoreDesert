using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
  public class TechNodeVehicleRemoteControl : TechNode<TechGroupVehiclesT3>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeVehicleRemoteControl>();

      config.SetRequiredNode<TechNodeHoverboardMk1>();
    }
  }
}