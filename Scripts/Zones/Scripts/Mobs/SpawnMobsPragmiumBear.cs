namespace AtomicTorch.CBND.CoreMod.Zones
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.Triggers;
  using System;

  public class SpawnMobsPragmiumBear : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          .Add(GetTrigger<TriggerWorldInit>())
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(30)));

      spawnList.CreatePreset(interval: 150, padding: 2.0, useSectorDensity: false)
               .AddExact<MobPragmiumBear>()
               .SetCustomPaddingWithSelf(65);
    }
  }
}