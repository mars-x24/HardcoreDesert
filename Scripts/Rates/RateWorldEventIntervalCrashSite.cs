using AtomicTorch.CBND.CoreMod.Events;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateWorldEventIntervalCrashSite
        : BaseRateWorldEventInterval<EventCrashSiteSpaceship, RateWorldEventIntervalCrashSite>
  {
    public override Interval<double> DefaultTimeIntervalHours => (min: 3.5, max: 5.0);
  }
}