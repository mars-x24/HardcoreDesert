using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  public class ItemStorageFridgePublicState : ItemStoragePublicState
  {
    [SyncToClient]
    public string IconOverlay { get; set; }

    [SyncToClient]
    public bool IsOn { get; set; }

    [SyncToClient]
    public double PowerPourcent { get; set; }

    [SyncToClient]
    public double LastPowerPourcent { get; set; }
  }
}