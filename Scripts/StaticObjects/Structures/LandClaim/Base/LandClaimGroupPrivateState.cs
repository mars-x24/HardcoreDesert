using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.State;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
  public class LandClaimGroupPrivateState : BasePrivateState
  {
    [SyncToClient]
    public IItemsContainer ItemsContainerGlobalStorage { get; set; }

    public List<ILogicObject> ServerLandClaimAreasGroups { get; set; }

    public List<string> owners { get; set; }

    public string FactionClanTag { get; set; }
  }
}