using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
  public class ItemBackpackHeavyPragmium : ProtoItemBackpack
  { 
    public override byte SlotsCount => 40;

    public override string Name => "Pragmium backpack";

    public override uint DurabilityMax => 50000;

    public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;

    protected override void PrepareEffects(Effects effects)
    {
      base.PrepareEffects(effects);

      effects
        .AddPercent(this, StatName.CraftingSpeed, 5)
        .AddPercent(this, StatName.MiningSpeed, 5)
        .AddPercent(this, StatName.WoodcuttingSpeed, 5)
        .AddPercent(this, StatName.ForagingSpeed, 5)
        .AddPercent(this, StatName.FarmingTasksSpeed, 5);

    }
  }
}