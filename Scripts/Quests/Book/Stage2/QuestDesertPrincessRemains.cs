namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestDesertPrincessRemains : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting! This time not even just animals!";

    public override string Name => "Desert Princess Remains";

    public override string Hints => "The Princess can be found in the barren.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage2;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      ITextureResource icon = null;

      if (IsClient)
        icon = Api.GetProtoEntity<ObjectDesertPrincessRemains>().Icon;

      tasks
           .Add(TaskDestroy.Require<ObjectDesertPrincessRemains>(count: 1).WithIcon(icon));

      prerequisites
           .Add<QuestKillPragmiumBears>();
    }
  }
}