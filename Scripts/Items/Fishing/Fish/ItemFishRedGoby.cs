namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
  using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using System;

  public class ItemFishRedGoby : ProtoItemFish
  {
    public override string Description => GetProtoEntity<ItemFishBass>().Description;

    public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

    public override bool IsSaltwaterFish => false;

    public override bool IsLavaFish => true;

    public override float MaxLength => 50;

    public override float MaxWeight => 8f;

    public override string Name => "Red goby";

    public override byte RequiredFishingKnowledgeLevel => 85;

    protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
    {
      baitWeightList.Add<ItemFishingPragmiumBaitMix>(weight: 1);

      dropItemsList.Add<ItemKeiniteRaw>(count: 6, countRandom: 1)
                   .Add<ItemGemstones>(count: 1, probability: 0.05)
                   .Add<ItemSulfurPowder>(count: 10, countRandom: 5);
    }
  }
}