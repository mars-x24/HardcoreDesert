namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Alien
{
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;

  public class ObjectPropAlienTeleportPragmiumKing : ProtoObjectProp
  {
    public override bool CanFlipSprite => true;

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override StaticObjectKind Kind => StaticObjectKind.Floor;

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrder = DrawOrder.Floor;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("####",
                   "####",
                   "####");
    }
    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {

    }
  }
}