namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
  using AtomicTorch.CBND.CoreMod.Items.Generic;
  using AtomicTorch.CBND.CoreMod.Skills;
  using AtomicTorch.CBND.CoreMod.Systems.Droplists;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System;

  /// <summary>
  /// This special pragmium lava fish
  /// </summary>
  public class ItemFishRedGlider : ProtoItemFish
  {
    public const int SkillFishingLevelRequired = 15;

    public override string Description => "Unique pragmium-based lava lifeform native to this world.";

    public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

    public override bool IsSaltwaterFish => false;

    public override bool IsLavaFish => true;

    public override float MaxLength => 100;

    public override float MaxWeight => 15;

    public override string Name => "Red glider";

    // Please note: it's not used as a different check is performed (character skill level). 
    public override byte RequiredFishingKnowledgeLevel => 85;

    public override bool ServerCanCatch(ICharacter character, Vector2Ushort fishingTilePosition)
    {
      return character.SharedHasSkill<SkillFishing>(SkillFishingLevelRequired);
    }

    protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
    {
      baitWeightList.Add<ItemFishingPragmiumBaitMix>(weight: 1);

      dropItemsList.Add<ItemOrePragmium>(count: 3, countRandom: 1)
                   .Add<ItemGemstones>(count: 1, probability: 0.05)
                   .Add<ItemSulfurPowder>(count: 10, countRandom: 5);
    }
  }
}