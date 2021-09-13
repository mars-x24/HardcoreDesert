namespace AtomicTorch.CBND.CoreMod.Rates
{
  using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesClient;
  using AtomicTorch.GameEngine.Common.Helpers;
  using System;

  public class RateMigrationMutantMobMaxLevelPerWave
      : BaseRate<RateMigrationMutantMobMaxLevelPerWave, string>
  {
    public static ushort[] SharedValues { get; private set; }

    public override string Description => @"Max level of mobs for each wave.";

    public override string Id => "MigrationMutant.MaxLevelPerWave";

    public override string Name => "Mutant migration mobs max level per wave.";

    public override string ValueDefault => "1,2,3,4,5";

    public override RateVisibility Visibility => RateVisibility.Primary;

    public override IViewModelRate ClientCreateViewModel()
    {
      return new ViewModelRateString(this);
    }

    protected override void ClientOnValueChanged()
    {
      SharedValues = ParseMobMaxLevelPerWave(SharedValue);
    }

    protected override string ServerReadValue()
    {
      var currentValue = ServerRatesApi.Get(this.Id, this.ValueDefault, this.Description);

      try
      {
        SharedValues = ParseMobMaxLevelPerWave(currentValue);
      }
      catch
      {
        Api.Logger.Error(
            $"Incorrect format for server rate: {this.Id} current value {currentValue}. Please note that the values must be separated by comma and each value must be NOT higher than 50.");
        ServerRatesApi.Reset(this.Id, this.ValueDefault, this.Description);
        currentValue = this.ValueDefault;
        SharedValues = ParseMobMaxLevelPerWave(currentValue);
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

    private static ushort[] ParseMobMaxLevelPerWave(string str)
    {
      ushort[] ret;

      string[] mobMaxLevelSplit = str.Replace(" ", "").Split(',');
      
      ret = Array.ConvertAll(mobMaxLevelSplit, s => ushort.Parse(s));

      for (int i = 0; i < ret.Length; i++)
        ret[i] = MathHelper.Clamp(ret[i], 1, 5);

      return ret;
    }

    public static ushort GetMaxLevelForWaveNumber(byte waveNumber)
    {
      if (RateMigrationMutantMobMaxLevelPerWave.SharedValues.Length != RateMigrationMutantWaveCount.SharedValue)
        return 1;

      return RateMigrationMutantMobMaxLevelPerWave.SharedValues[waveNumber];
    }
  }
}