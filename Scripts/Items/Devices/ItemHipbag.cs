namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  public class ItemHipbag : ProtoItemBackpack
  {
    public override string Description =>
        "Additional hip bag to store your belongings.";

    public override byte SlotsCount => 10;

    public override string Name => "Hip bag";

    public override uint DurabilityMax => 20000;

    public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;


  }
}