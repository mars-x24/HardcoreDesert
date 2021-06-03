using AtomicTorch.CBND.CoreMod.Vehicles;

namespace AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes
{
  public class ItemLargePickaxeSteel : ProtoItemVehicleToolPickaxe
  {
    public override double DamageApplyDelay => 0.075;

    public override double DamageToMinerals => 270;

    public override double DamageToNonMinerals => 60;

    public override string Description =>
        "Large Steel pickaxe can be used on vehicle to mine mineral deposits.";

    public override uint DurabilityMax => 1200;

    public override double FireAnimationDuration => 0.6;

    public override string Name => "Large Steel pickaxe";

    public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Tool;
  }
}