using AtomicTorch.CBND.CoreMod.Vehicles;

namespace AtomicTorch.CBND.CoreMod.Items.Tools.Axes
{
  public class ItemLargeAxeSteel : ProtoItemVehicleToolAxe
  {
    public override double DamageApplyDelay => 0.075;

    public override double DamageToNonTree => 60;

    public override double DamageToTree => 210;

    public override string Description
        => "Large Steel axe is ideal for chopping trees. Can be used on vehicle.";

    public override uint DurabilityMax => 1000;

    public override double FireAnimationDuration => 0.6;

    public override string Name => "Large Steel axe";

    public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Tool;
  }
}