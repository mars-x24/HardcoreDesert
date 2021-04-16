namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
  using System.Collections.Generic;

  public class ItemVehicleRemoteControl : ProtoItemVehicleRemoteControl
  {
    public override string Description =>
        "This remote allows you to recall your vehicle near you in exchange of a good dose of energy.";

    public override uint DurabilityMax => 30;

    public override string Name => "Vehicle remote control";

    public const string UsesPowerBanks = "This item draws {0} EU from the equipped powerbanks.";

    public const string SelectVehicle = "Click the vehicle icon to select a one.";

    protected override void PrepareHints(List<string> hints)
    {
      base.PrepareHints(hints);
      hints.Add(string.Format(UsesPowerBanks, this.EngeryUse.ToString("#")));
      hints.Add(SelectVehicle);
    }
  }
}