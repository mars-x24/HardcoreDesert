using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateVehicleBackupWeaponEnabled
      : BaseRateBoolean<RateVehicleBackupWeaponEnabled>
  {
    [NotLocalizable]
    public override string Description =>
        @"Determines whether the backup weapon in vehicle is available.";

    public override string Id => "VehicleBackupWeapon.Enabled";

    public override string Name => "Vehicle backup weapon available";

    public override bool ValueDefault => true;

    public override RateVisibility Visibility => RateVisibility.Advanced;
  }
}