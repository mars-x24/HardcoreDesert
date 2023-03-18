using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Teleport
{
  public class TechNodeTeleport : TechNode<TechGroupTeleportT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddStructure<ObjectAlienTeleportReplicaPosition>();

      config.SetRequiredNode<TechNodeTeleportAlien2>();
    }
  }
}