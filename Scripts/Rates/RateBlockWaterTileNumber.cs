using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateBlockWaterTileNumber
      : BaseRateByte<RateBlockWaterTileNumber>
  {
    [NotLocalizable]
    public override string Description => @"Number of tiles to block around world limits.";

    public override string Id => "BlockWateTileNumber";

    public override string Name => "Block water tile number";

    public override byte ValueDefault => 10;

    public override byte ValueMax => 100;

    public override byte ValueMin => 1;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Advanced;
  }
}