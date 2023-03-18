using AtomicTorch.CBND.CoreMod.Vehicles;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles
{
  public class TechNodeHovercraftMk1 : TechNode<TechGroupVehiclesT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddVehicle<VehicleHovercraftMk1>();

      //config.SetRequiredNode<TechNodeHoverboardMk1>();
    }
  }
}