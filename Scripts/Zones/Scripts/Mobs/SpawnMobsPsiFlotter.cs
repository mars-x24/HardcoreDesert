using AtomicTorch.CBND.CoreMod.Characters.Mobs;
using AtomicTorch.CBND.CoreMod.Triggers;
using System;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class SpawnMobsPsiFlotter : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

      spawnList.CreatePreset(interval: 10, padding: 1.0)
               .AddExact<MobPsiFloater>()
               .SetCustomPaddingWithSelf(5);
    }
  }
}