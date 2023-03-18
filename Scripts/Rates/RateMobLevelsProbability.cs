using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.ServicesClient;
using AtomicTorch.GameEngine.Common.Helpers;
using System;

namespace AtomicTorch.CBND.CoreMod.Rates
{
  public class RateMobLevelsProbability
      : BaseRate<RateMobLevelsProbability, string>
  {
    public static ushort[] SharedValues { get; private set; }

    public override string Description => @"Mob levels probability, increase number occurence for more probability.";

    public override string Id => "MobLevelsProbability";

    public override string Name => "Mob levels probability";

    public override string ValueDefault => "1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,3,3,3,3,4,4,5";

    public override RateVisibility Visibility => RateVisibility.Advanced;

    public override IViewModelRate ClientCreateViewModel()
    {
      return new ViewModelRateString(this);
    }

    protected override void ClientOnValueChanged()
    {
      SharedValues = ParseValue(SharedValue);
    }

    protected override string ServerReadValue()
    {
      var currentValue = ServerRatesApi.Get(this.Id, this.ValueDefault, this.Description);

      try
      {
        SharedValues = ParseValue(currentValue);
      }
      catch
      {
        Api.Logger.Error(
            $"Incorrect format for server rate: {this.Id} current value {currentValue}. Please note that the values must be separated by comma and each value must be NOT higher than 5.");
        ServerRatesApi.Reset(this.Id, this.ValueDefault, this.Description);
        currentValue = this.ValueDefault;
        SharedValues = ParseValue(currentValue);
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

    private static ushort[] ParseValue(string str)
    {
      ushort[] ret;

      string[] split = str.Replace(" ", "").Split(',');

      ret = Array.ConvertAll(split, s => ushort.Parse(s));

      for (int i = 0; i < ret.Length; i++)
        ret[i] = MathHelper.Clamp(ret[i], 1, 5);

      return ret;
    }
  }
}