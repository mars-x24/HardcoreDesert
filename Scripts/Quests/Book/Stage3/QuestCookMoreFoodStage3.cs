namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestCookMoreFoodStage3 : ProtoQuest
  {
    public override string Description =>
        "Now that you've built a campfire, you can use it to cook some basic food.";

    public override string Name => "Cook more food";

    public override string Hints => "You are always hungry.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage3;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      var task = new TaskManufactureItem(QuestCookMoreFoodStage2.GetList(), count: 200, description: this.Name);

      tasks
          .Add(task);

      prerequisites
          .Add<QuestCookMoreFoodStage2>()
          .Add<QuestFishing>();
    }
  }
}