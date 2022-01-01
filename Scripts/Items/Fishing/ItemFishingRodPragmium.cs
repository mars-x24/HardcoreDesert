namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
  using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System.Windows.Media;

  public class ItemFishingRodPragmium : ProtoItemFishingRod
  {
    public override string Description =>
        "This fishing rod can be used to catch fish in unusual environments.";

    public override uint DurabilityMax => 200;

    public override Vector2F FishingLineStartScreenOffset => (47, 283);

    public override double FishingSpeedMultiplier => 0.6;

    public override string Name => "Pragmium fishing rod";

    public override bool IsFishingLava => true;

    public override Color LineColor => Color.FromArgb(0x55, 0xAA, 0xAA, 0xFF);

    public override ITextureAtlasResource TextureResourceFishingFloat
        => new TextureAtlasResource("FX/Fishing/FloatPragmium.png", columns: 6, rows: 1, isTransparent: true);
  }
}