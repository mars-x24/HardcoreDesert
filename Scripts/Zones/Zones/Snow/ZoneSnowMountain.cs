using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneSnowMountain : ProtoZoneDefault
  {
    [NotLocalizable]
    public override string Name => "Snow - Mountain";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      // minerals
      scripts
          .Add(GetScript<SpawnResourceCopper>())
          .Add(GetScript<SpawnResourceIron>())
          .Add(GetScript<SpawnResourceStone>())
          .Add(GetScript<SpawnResourceSaltpeter>())
          .Add(GetScript<SpawnResourceSulfur>());

      // loot
      scripts
          .Add(GetScript<SpawnLootPileMinerals>());

      // mobs
      scripts
          .Add(GetScript<SpawnMobsPsiGrove>())
          .Add(GetScript<SpawnMobsSnow>().Configure(densityMultiplier: 0.5));
    }
  }
}