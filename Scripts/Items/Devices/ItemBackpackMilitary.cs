namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  public class ItemBackpackMilitary : ProtoItemBackpack
  { 
    public override byte SlotsCount => 30;

    public override string Name => "Military backpack";

    public override uint DurabilityMax => 40000;

    public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;
  }
}