namespace AtomicTorch.CBND.CoreMod.Zones
{
  using AtomicTorch.CBND.GameApi;

  public class ZoneGiantPragmiumSource : ProtoZoneDefault
  {
    [NotLocalizable]
    public override string Name => "Special - Giant pragmium source spawn";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      scripts
          .Add(GetScript<SpawnGiantPragmiumSource>());
    }

  }
}