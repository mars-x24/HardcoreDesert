namespace AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class ContainerMechEquipmentCrusher : BaseItemsContainerMechEquipment
    {
        public override byte AmmoSlotsCount => 3;

        public override VehicleWeaponHardpoint WeaponHardpointName => VehicleWeaponHardpoint.Tool;

        public override byte WeaponSlotsCount => 1;
    }
}