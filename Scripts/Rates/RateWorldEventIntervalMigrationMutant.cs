using AtomicTorch.CBND.CoreMod.Events;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateWorldEventIntervalMigrationMutant
      : BaseRateWorldEventInterval<EventMigrationMutant, RateWorldEventIntervalMigrationMutant>
  {
    public override Interval<double> DefaultTimeIntervalHours => (min: 1.0, max: 1.5);
  }
}