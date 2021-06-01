namespace AtomicTorch.CBND.CoreMod.Zones
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using System;

  public class SpawnMobDesertPrincess : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(30)));

      spawnList.CreatePreset(interval: 140, padding: 4, useSectorDensity: false)
               .AddExact<MobDesertPrincess>()
               .SetCustomPaddingWithSelf(70);
    }
  }
}