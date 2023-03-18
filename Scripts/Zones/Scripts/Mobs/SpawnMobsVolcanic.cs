﻿using AtomicTorch.CBND.CoreMod.Characters.Mobs;
using AtomicTorch.CBND.CoreMod.Triggers;
using System;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class SpawnMobsVolcanic : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

      var crawler = spawnList.CreatePreset(interval: 12, padding: 0.5)
                             .AddExact<MobCrawler>()
                             .SetCustomPaddingWithSelf(12);

      var fireLizard = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                .AddExact<MobFireLizard>()
                                .SetCustomPaddingWith(crawler, 15)
                                .SetCustomPaddingWithSelf(15);

      var blackBeetle = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                 .AddExact<MobBlackBeetle>()
                                 .SetCustomPaddingWith(crawler, 15)
                                 .SetCustomPaddingWith(fireLizard, 15)
                                 .SetCustomPaddingWithSelf(15);

      var pragbear = spawnList.CreatePreset(interval: 70, padding: 1.5, spawnAtLeastOnePerSector: true)
                              .AddExact<MobLargePragmiumBear>()
                              .SetCustomPaddingWith(crawler, 15)
                              .SetCustomPaddingWith(fireLizard, 15)
                              .SetCustomPaddingWithSelf(79);
    }
  }
}