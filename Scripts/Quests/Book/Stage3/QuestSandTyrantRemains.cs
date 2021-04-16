namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestSandTyrantRemains : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting! This time not even just animals!";

    public override string Name => "Sand Tyrant Remains";

    public override string Hints => "This boss rarely appears in craters around the world";
    
    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage3;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      ITextureResource icon = null;

      if (IsClient)
        icon = Api.GetProtoEntity<ObjectSandTyrantRemains>().Icon;

      tasks
           .Add(TaskDestroy.Require<ObjectSandTyrantRemains>(count: 1).WithIcon(icon));

      prerequisites
           .Add<QuestDesertPrincessRemains>();
    }
  }
}