namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using System;

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