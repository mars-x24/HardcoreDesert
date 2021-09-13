namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.State;
  using System.Collections.Generic;

  public class ObjectHackableContainerPrivateState : BasePrivateState
  {
    public List<ICharacter> MobsList { get; } = new();      
  }
}