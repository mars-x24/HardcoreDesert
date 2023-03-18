using AtomicTorch.CBND.GameApi.Data;
using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  public class ItemStoragePublicState : BasePublicState
  {
    [SyncToClient]
    public IProtoEntity IconSource { get; set; }
  }
}