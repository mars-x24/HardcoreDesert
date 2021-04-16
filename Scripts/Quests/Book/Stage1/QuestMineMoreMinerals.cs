namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestMineMoreMinerals : ProtoQuest
  {
    public override string Description =>
        "Let's put this pickaxe to use. Mine some minerals and see what you get.";

    public override string Name => "Mine more minerals";

    public override string Hints => "The best place to find mineral nodes is in the [b]mountains[/b].";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage1;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      var listMinerals = Api.FindProtoEntities<IProtoObjectMineral>();

      tasks.Add(TaskDestroy.Require(list: listMinerals, count: 250, description: this.Name));

      prerequisites
          .Add<QuestMineAnyMineral>();
    }
  }
}