namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using System.Collections.Generic;

  public class LootContainerPrivateState : ObjectCratePrivateState
  {
    public bool IsDropListSpawned { get; set; }

    public List<ICharacter> MobsList { get; } = new();
  }
}