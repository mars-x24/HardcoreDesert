namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
  using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using System;

  public class ItemFishingPragmiumBaitMix : ProtoItemFishingBait, IProtoItemOrganic
  {
    public override string Description =>
        "Pragmium boilies are a special mixed bait combining many different ingredients, which makes it suitable for some unique types of fish.";

    public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

    public override string Name => "Pragmium boilie bait";

    public ushort OrganicValue => 1;
  }
}