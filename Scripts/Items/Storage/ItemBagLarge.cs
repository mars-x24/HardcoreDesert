namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  public class ItemBagLarge : ProtoItemStorage
  { 
    public override byte SlotsCount => 16;

    public override int MaxItemCount => 200;

    public override int MaxItemCountPerType => 20;

    public override string Name => "Large bag";

  }
}