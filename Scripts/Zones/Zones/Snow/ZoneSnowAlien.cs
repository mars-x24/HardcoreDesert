namespace AtomicTorch.CBND.CoreMod.Zones
{
  using AtomicTorch.CBND.GameApi;

  public class ZoneSnowAlien : ProtoZoneDefault
  {
    [NotLocalizable]
    public override string Name => "Snow - Alien";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      // loot
      scripts
          .Add(GetScript<SpawnLootRuinsLaboratory>());

      // mobs
      scripts
          .Add(GetScript<SpawnMobsPsiFlotter>());
    }
  }
}