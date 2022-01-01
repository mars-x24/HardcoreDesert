namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
  using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.GameEngine.Common.Primitives;

  public class ItemFishingRodDouble : ProtoItemFishingRod
  {
    public override string Description =>
        "This fishing rod can be used to catch fish in any body of water with 2 baits at the same time.";

    public override uint DurabilityMax => 100;

    public override Vector2F FishingLineStartScreenOffset => (47, 283);

    public override double FishingSpeedMultiplier => 1.0;

    public override string Name => "Double bait fishing rod";

    public override byte BaitCount => 2;

    public override ITextureAtlasResource TextureResourceFishingFloat
        => new TextureAtlasResource("FX/Fishing/FloatDouble.png", columns: 6, rows: 1, isTransparent: true);
  }
}