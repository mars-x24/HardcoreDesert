using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateConstructionOverlayAlwaysVisible
        : BaseRateBoolean<RateConstructionOverlayAlwaysVisible>
  {
    [NotLocalizable]
    public override string Description =>
        @"Determines if you see the construction overlay when building or relocating structures.
              By default to true like vanilla game.
              Change to 0 to disable overlay. You can still hold ALT to see it.";

    public override string Id => "ConstructionOverlayAlwaysVisible";

    public override string Name => "Construction overlay always visible";

    public override bool ValueDefault => true;

    public override RateVisibility Visibility => RateVisibility.Advanced;
  }
}