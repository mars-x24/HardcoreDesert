namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.State;

  public class ObjectGlobalChestPublicState : ObjectCratePublicState
  {
    [SyncToClient]
    public ILogicObject LandClaimGroup { get; set; }
  }
}