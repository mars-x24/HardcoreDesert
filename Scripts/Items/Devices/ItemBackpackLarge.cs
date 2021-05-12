namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  public class ItemBackpackLarge : ProtoItemBackpack
  {

    public override byte SlotsCount => 20;

    public override string Name => "Cloth backpack";

    public override uint DurabilityMax => 30000;

    public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;


  }
}