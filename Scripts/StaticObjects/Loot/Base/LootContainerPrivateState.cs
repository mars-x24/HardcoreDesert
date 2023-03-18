using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
using AtomicTorch.CBND.GameApi.Data.Characters;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
  public class LootContainerPrivateState : ObjectCratePrivateState
  {
    public bool IsDropListSpawned { get; set; }

    public List<ICharacter> MobsList { get; } = new();
  }
}