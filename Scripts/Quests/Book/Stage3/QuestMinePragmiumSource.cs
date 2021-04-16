namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestMinePragmiumSource : ProtoQuest
  {
    public override string Description =>
        "Let's put this pickaxe to use. Mine some minerals and see what you get.";

    public override string Name => "Mine more minerals";

    public override string Hints => "Beware! Mining pragmium is very dangerous.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage3;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks.Add(TaskDestroy.Require<ObjectMineralPragmiumSource>(count: 5)); ;

      prerequisites
          .Add<QuestMineMoreMineralsStage3>()
          .Add<QuestAcquirePragmium>();
    }
  }
}