using AtomicTorch.CBND.CoreMod.Characters.Mobs;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class SpawnEventMutantCrawlersInfestation : ProtoZoneSpawnScript
  {
    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      // no triggers, only manual triggering through the event when it starts
      spawnList.CreatePreset(interval: 10, padding: 0.5, useSectorDensity: false)
               .AddExact<MobMutantCrawler>()
               .SetCustomPaddingWithSelf(8);
    }
  }
}