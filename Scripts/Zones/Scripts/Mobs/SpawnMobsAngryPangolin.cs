using AtomicTorch.CBND.CoreMod.Characters.Mobs;
using AtomicTorch.CBND.CoreMod.Triggers;
using System;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class SpawnMobsAngryPangolin : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

      spawnList.CreatePreset(interval: 55, padding: 1.0, useSectorDensity: false)
               .AddExact<MobAngryPangolin>()
               .SetCustomPaddingWithSelf(50);
    }
  }
}