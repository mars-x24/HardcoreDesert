namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  public class ItemBagSmall : ProtoItemStorage
  {
    public override byte SlotsCount => 8;

    public override int MaxItemCount => 100;

    public override int MaxItemCountPerType => 10;

    public override string Name => "Small bag";
  }
}