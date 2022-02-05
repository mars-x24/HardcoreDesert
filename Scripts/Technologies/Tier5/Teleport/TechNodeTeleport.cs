namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Teleport
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;

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