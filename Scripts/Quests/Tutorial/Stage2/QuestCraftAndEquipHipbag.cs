namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
  using AtomicTorch.CBND.CoreMod.Items.Devices;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Collections.Generic;

  public class QuestCraftAndEquipHipbag : ProtoQuest
  {
    public override string Description =>
        "You may need more inventory space when running around. Having a hip bag can be useful to increase your inventory size. Research, craft and equip hip bag.";

    public override string Hints =>
        @"[*] Hip bag must be equipped as a device.
          [*] You can only have 1 equipped device increasing your inventory size.
          [*] Watch your hip bag's durability. Your pockets can't hold all the extra items if it break.";

    public override string Name => "Craft and equip hip bag";

    public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      var protoItemHipbag = Api.GetProtoEntity<ItemHipbag>();

      tasks
          .Add(TaskHaveTechNode.Require<TechNodeHipbag>())

          .Add(TaskHaveItemEquipped.Require(new List<ProtoItemBackpack>()
                                   {
                                       Api.GetProtoEntity<ItemHipbag>(),
                                       Api.GetProtoEntity<ItemBackpackLarge>(),
                                       Api.GetProtoEntity<ItemBackpackMilitary>()
                                   }, this.Name));

      prerequisites
          .Add<QuestCraftAndEquipClothArmor>();
    }
  }
}