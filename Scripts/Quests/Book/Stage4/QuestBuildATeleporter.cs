using AtomicTorch.CBND.CoreMod.CraftRecipes;
using AtomicTorch.CBND.CoreMod.PlayerTasks;
using HardcoreDesert.Scripts.Quests.Base;

namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  public class QuestBuildATeleporter : ProtoQuest
  {
    public override string Description => "Time to build a teleporter where you prefer!";

    public override string Name => "Teleport replica";

    public override string Hints => "Once built, a teleporter can't be moved or destroyed, so choose wisely.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage4;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskCraftRecipe.RequireStationRecipe<RecipeTeleportAlien1>(count: 4))
           .Add(TaskCraftRecipe.RequireStationRecipe<RecipeTeleportAlien2>(count: 4))
           .Add(TaskCraftRecipe.RequireStationRecipe<RecipeTeleportAlien3>(count: 12))
           .Add(TaskCraftRecipe.RequireStationRecipe<RecipeTeleportAlien4>(count: 4));

      prerequisites
           .Add<QuestKillPsiFloater>();
    }
  }
}