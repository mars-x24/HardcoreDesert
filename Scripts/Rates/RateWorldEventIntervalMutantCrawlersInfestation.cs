using AtomicTorch.CBND.CoreMod.Events;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateWorldEventIntervalMutantCrawlersInfestation
      : BaseRateWorldEventInterval<EventMutantCrawlersInfestation, RateWorldEventIntervalMutantCrawlersInfestation>
  {
    public override Interval<double> DefaultTimeIntervalHours => (min: 3.0, max: 5.0);
  }
}