using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting;

namespace AtomicTorch.CBND.CoreMod.Vehicles
{
  public class VehicleWreckedHoverboardMk2 : VehicleHoverboardMk2
  {
    public override byte FuelItemsSlotsCount => 1;

    public override string InteractionTooltipText => "Enter";

    public override string Name => "Wrecked Hoverboard Mk2";

    protected override void ClientInteractStart(ClientObjectData data)
    {
      //base.ClientInteractStart(data);

      VehicleSystem.ClientOnVehicleEnterOrExitRequest();
    }

    public override TextureResource TextureResourceHoverboard { get; }
        = new("Vehicles/WreckedHoverboardMk2");

    public override TextureResource TextureResourceHoverboardLight { get; }
        = new("Vehicles/WreckedHoverboardMk2Light");

    public override BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
    {
      return null;
    }

    protected override void ServerUpdateVehicle(ServerUpdateData data)
    {
      base.ServerUpdateVehicle(data);

      var energy = VehicleEnergySystem.SharedCalculateTotalEnergyCharge(data.GameObject);
      if (energy <= 0)
        Api.Server.World.DestroyObject(data.GameObject);
    }

  }
}