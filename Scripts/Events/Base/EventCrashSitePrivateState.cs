namespace AtomicTorch.CBND.CoreMod.Events
{
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System.Collections.Generic;

  public class EventCrashSitePrivateState : BasePrivateState
  {
    public IWorldObject SpawnedCrashObject { get; set; }

    public Vector2Ushort SpawnedCrashObjectPosition { get; set; }

    public List<IWorldObject> SpawnedWorldObjects { get; } = new();

    public void Init()
    {
      this.ClearList(this.SpawnedWorldObjects);
    }

    private void ClearList(List<IWorldObject> list)
    {
      for (var index = 0; index < list.Count; index++)
      {
        var worldObject = list[index];
        if (worldObject is null)
          list.RemoveAt(index--);
      }
    }
  }
}