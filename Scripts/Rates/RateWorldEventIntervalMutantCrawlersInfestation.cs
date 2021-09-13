namespace AtomicTorch.CBND.CoreMod.Rates
{
  using AtomicTorch.CBND.CoreMod.Events;
  using AtomicTorch.GameEngine.Common.Primitives;

  public class RateWorldEventIntervalMutantCrawlersInfestation
      : BaseRateWorldEventInterval<EventMutantCrawlersInfestation, RateWorldEventIntervalMutantCrawlersInfestation>
  {
    public override Interval<double> DefaultTimeIntervalHours => (min: 3.0, max: 5.0);
  }
}