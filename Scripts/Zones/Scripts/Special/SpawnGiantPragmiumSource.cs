﻿using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
using AtomicTorch.CBND.CoreMod.Triggers;
using System;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class SpawnGiantPragmiumSource : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromHours(1)));

      spawnList.CreatePreset(interval: 10, padding: 10, useSectorDensity: false, spawnAtLeastOnePerSector: true)
               .Add<ObjectMineralGiantPragmiumSource>().SetCustomPaddingWithSelf(79);
    }
  }
}