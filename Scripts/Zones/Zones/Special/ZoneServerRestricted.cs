using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneServerRestricted : ProtoZoneDefault
  {
    public static ZoneServerRestricted Instance { get; private set; }

    [NotLocalizable]
    public override string Name => "Server - Restricted";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      // Server restricted area
      Instance = this;
    }
  }
}