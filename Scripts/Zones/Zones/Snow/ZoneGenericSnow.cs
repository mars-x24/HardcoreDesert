using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneGenericSnow : ProtoZoneDefault
  {
    [NotLocalizable]
    public override string Name => "Snow - Barren";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      // minerals
      scripts
          .Add(GetScript<SpawnResourcePragmium>())
          .Add(GetScript<SpawnResourceCoal>().Configure(densityMultiplier: 0.25));

      // mobs
      scripts
          .Add(GetScript<SpawnMobsSnow>())
          .Add(GetScript<SpawnMobsBroodNest>());
    }
  }
}