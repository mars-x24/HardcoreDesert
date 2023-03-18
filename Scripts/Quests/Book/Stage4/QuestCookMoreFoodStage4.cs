﻿using AtomicTorch.CBND.CoreMod.PlayerTasks;
using HardcoreDesert.Scripts.Quests.Base;

namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  public class QuestCookMoreFoodStage4 : ProtoQuest
  {
    public override string Description =>
        "Now that you've built a campfire, you can use it to cook some basic food.";

    public override string Name => "Cook more food";

    public override string Hints => "You are always hungry.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage4;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      var task = new TaskManufactureItem(QuestCookMoreFoodStage2.GetList(), count: 500, description: this.Name);

      tasks
          .Add(task);

      prerequisites
          .Add<QuestCookMoreFoodStage3>();
    }
  }
}