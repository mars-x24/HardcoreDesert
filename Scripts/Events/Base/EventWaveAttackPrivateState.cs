using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Events
{
  public class EventWaveAttackPrivateState : BasePrivateState
  {
    public List<IWorldObject> SpawnedWorldObjects { get; }
        = new();

    public void Init()
    {
      for (var index = 0; index < this.SpawnedWorldObjects.Count; index++)
      {
        var worldObject = this.SpawnedWorldObjects[index];
        if (worldObject is null)
        {
          this.SpawnedWorldObjects.RemoveAt(index--);
        }
      }
    }
  }
}