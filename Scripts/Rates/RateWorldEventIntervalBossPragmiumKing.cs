using AtomicTorch.CBND.CoreMod.Events;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateWorldEventIntervalBossPragmiumKing
        : BaseRateWorldEventInterval<EventBossPragmiumKing, RateWorldEventIntervalBossPragmiumKing>
  {
    public override Interval<double> DefaultTimeIntervalHours => (min: 12, max: 24);
  }
}