using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.State;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  public class ObjectMineralMeteoritePrivateState : BasePrivateState
  {
    public List<ICharacter> MobsList { get; } = new();      
  }
}