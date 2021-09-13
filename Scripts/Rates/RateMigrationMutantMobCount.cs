namespace AtomicTorch.CBND.CoreMod.Rates
{
  using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesClient;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System;

  public class RateMigrationMutantMobCount
      : BaseRate<RateMigrationMutantMobCount, string>
  {
    public static ushort[] SharedValues { get; private set; }

    public override string Description => @"Number of mobs for each claims (T1 to T5).";

    public override string Id => "MigrationMutant.MobCounts";

    public override string Name => "Mutant migration mob counts";

    public override string ValueDefault => "1,4,8,13,20";

    public override RateVisibility Visibility => RateVisibility.Primary;

    public override IViewModelRate ClientCreateViewModel()
    {
      return new ViewModelRateString(this);
    }

    protected override void ClientOnValueChanged()
    {
      SharedValues = ParseMobCounts(SharedValue);
    }

    protected override string ServerReadValue()
    {
      var currentValue = ServerRatesApi.Get(this.Id, this.ValueDefault, this.Description);

      try
      {
        SharedValues = ParseMobCounts(currentValue);
      }
      catch
      {
        Api.Logger.Error(
            $"Incorrect format for server rate: {this.Id} current value {currentValue}. Please note that the values must be separated by comma and each value must be NOT higher than 50.");
        ServerRatesApi.Reset(this.Id, this.ValueDefault, this.Description);
        currentValue = this.ValueDefault;
        SharedValues = ParseMobCounts(currentValue);
      }

      return currentValue;
    }

    protected override void SharedApplyAbstractValueToConfig(IServerRatesConfig ratesConfig, string value)
    {
      ratesConfig.Set(this.Id,
                      value,
                      this.ValueDefault,
                      this.Description);
    }

    private static ushort[] ParseMobCounts(string str)
    {
      ushort[] ret;

      string[] mobCountSplit = str.Replace(" ", "").Split(',');
      if (mobCountSplit.Length != 5)
        ret = new ushort[] { 1, 4, 8, 13, 20 };
      else
        ret = Array.ConvertAll(mobCountSplit, s => ushort.Parse(s));

      for (int i = 0; i < ret.Length; i++)
        ret[i] = MathHelper.Clamp(ret[i], 0, 50);

      return ret;
    }
  }
}