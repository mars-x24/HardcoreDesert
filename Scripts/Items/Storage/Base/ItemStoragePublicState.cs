namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ItemStoragePublicState : BasePublicState
  {
    [SyncToClient]
    public IProtoEntity IconSource { get; set; }
  }
}