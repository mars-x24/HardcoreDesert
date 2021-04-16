namespace AtomicTorch.CBND.CoreMod.Zones
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using System;

  public class SpawnGiantPragmiumSource : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromHours(3)));

      spawnList.CreatePreset(interval: 10, padding: 10)
               .Add<ObjectMineralGiantPragmiumSource>().SetCustomPaddingWithSelf(79);
    }
  }
}