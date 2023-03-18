using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RatePvPSafeStorageCapacity
        : BaseRateByte<RatePvPSafeStorageCapacity>
  {
    [NotLocalizable]
    public override string Description =>
        @"How many safe storage slots are allowed per base.";

    public override string Id => "PvP.SafeStorageCapacity";

    public override string Name => "Land claim safe storage capacity";

    public override byte ValueDefault => 24;

    public override byte ValueMax => 128;

    public override byte ValueMin => 0;

    public override RateValueType ValueType => RateValueType.Number;

    public override RateVisibility Visibility => RateVisibility.Advanced;

    protected override byte ServerReadValueWithRange()
    {
      var result = base.ServerReadValueWithRange();
      //if (PveSystem.ServerIsPvE)
      //{
      //    result = 0; // no need for safe storage in PvE
      //}

      return result;
    }
  }
}