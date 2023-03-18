using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
  public class ObjectGlobalChestPublicState : ObjectCratePublicState
  {
    [SyncToClient]
    public ILogicObject LandClaimGroup { get; set; }
  }
}