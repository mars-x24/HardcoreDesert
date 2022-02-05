namespace AtomicTorch.CBND.CoreMod.Zones
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using System;

  public class SpawnMobsSnow : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

      var coldBear = spawnList.CreatePreset(interval: 35, padding: 0.5)
                             .AddExact<MobColdBear>()
                             .SetCustomPaddingWithSelf(40);

      var blackBeetle = spawnList.CreatePreset(interval: 20, padding: 0.5)
                                 .AddExact<MobBlackBeetle>()
                                 .SetCustomPaddingWithSelf(20);

      var frozenPangolin = spawnList.CreatePreset(interval: 35, padding: 0.5)
                           .AddExact<MobFrozenPangolin>()
                           .SetCustomPaddingWithSelf(40);

    }
  }
}