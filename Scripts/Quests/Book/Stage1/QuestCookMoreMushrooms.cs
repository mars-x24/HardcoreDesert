namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestCookMoreMushrooms : ProtoQuest
  {
    public override string Description =>
         "Now that you've built a campfire, you can use it to cook some basic food.";

    public override string Name => "Cook more mushrooms";

    public override string Hints => "You are always hungry.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage1;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
          .Add(TaskGather.Require<ObjectSmallMushroomPennyBun>(count: 5))
          .Add(TaskGather.Require<ObjectSmallMushroomRust>(count: 5))

          .Add(TaskUseItem.Require<ItemRoastedMushrooms>(count: 10, description: "Eat roasted mushrooms"));

      prerequisites
          .Add<QuestCookAnyFood>();
    }
  }
}