using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneSnowAlien : ProtoZoneDefault, IProtoNoAmbientLight
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