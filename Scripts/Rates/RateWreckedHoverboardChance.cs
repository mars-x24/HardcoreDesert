using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateWreckedHoverboardChance
      : BaseRateByte<RateWreckedHoverboardChance>
  {
    [NotLocalizable]
    public override string Description =>
     @"A chance to get wrecked hoverboard in garbage pile (1 / RateValue ;  Set 0 for none)";

    public override string Id => "WreckedHoverboardChance";

    public override string Name => "Wrecked Hoverboard Chance";

    public override byte ValueDefault => 30;

    public override byte ValueMax => 250;

    public override byte ValueMin => 0;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Advanced;
  }
}