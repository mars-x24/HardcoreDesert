namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;

  public interface IProtoItemStorage : IProtoItemUsableFromContainer, IProtoItemWithSlotOverlay
  {
    byte SlotsCount { get; }

    int MaxItemCount { get; }

    int MaxItemCountPerType { get; }

    void ClientSetIconSource(IItem itemStorage, IProtoEntity iconSource);
  }
}