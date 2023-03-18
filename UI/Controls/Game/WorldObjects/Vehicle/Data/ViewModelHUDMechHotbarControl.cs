using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
using AtomicTorch.CBND.CoreMod.Rates;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.World;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
  public class ViewModelHUDMechHotbarControl : BaseViewModel
  {
    public ViewModelHUDMechHotbarControl(IDynamicWorldObject vehicle)
    {
      var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;

      var structurePointsMax = protoVehicle.SharedGetStructurePointsMax(vehicle);
      this.ViewModelStructurePoints = new ViewModelStructurePointsBarControl()
      {
        ObjectStructurePointsData = new ObjectStructurePointsData(vehicle, structurePointsMax)
      };

      this.ViewModelVehicleEnergy = new ViewModelVehicleEnergy(vehicle);

      var privateState = vehicle.GetPrivateState<VehicleMechPrivateState>();
      this.EquipmentItemsContainer = privateState.EquipmentItemsContainer;
      this.EquipmentItemsContainerBackup = privateState.EquipmentItemsContainerBackup;
    }

    public IItemsContainer EquipmentItemsContainer { get; }

    public IItemsContainer EquipmentItemsContainerBackup { get; }

    public bool BackupWeaponAvailable
    {
      get => RateVehicleBackupWeaponEnabled.SharedValue;
    }

    public bool HasSecondWeaponSlot
            => ((BaseItemsContainerMechEquipment)this.EquipmentItemsContainer.ProtoItemsContainer)
               .WeaponSlotsCount
               > 1;

    public ViewModelStructurePointsBarControl ViewModelStructurePoints { get; }

    public ViewModelVehicleEnergy ViewModelVehicleEnergy { get; }
  }
}