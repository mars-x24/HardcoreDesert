using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  public class ItemBackpackHeavyKeinite : ProtoItemBackpack
  { 
    public override byte SlotsCount => 40;

    public override string Name => "Keinite backpack";

    public override uint DurabilityMax => 50000;

    public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;

    protected override void PrepareEffects(Effects effects)
    {
      base.PrepareEffects(effects);

      effects
         .AddPercent(this, StatName.HealthRegenerationPerSecond, 5)
         .AddPercent(this, StatName.HealthMax, 5)
         .AddPercent(this, StatName.StaminaMax, 5)
         .AddPercent(this, StatName.FoodMax, 5)
         .AddPercent(this, StatName.WaterMax, 5);


    }
  }
}