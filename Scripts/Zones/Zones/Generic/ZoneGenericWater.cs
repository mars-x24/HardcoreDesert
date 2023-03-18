using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneGenericWater : ProtoZoneDefault
  {
    [NotLocalizable]
    public override string Name => "Generic - Water";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      // loot
      scripts
          .Add(GetScript<SpawnLootWater>());
    }
  }
}