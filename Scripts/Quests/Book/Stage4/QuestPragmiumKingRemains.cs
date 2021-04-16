namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestPragmiumKingRemains : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting! This time not even just animals!";

    public override string Name => "Pragmium King Remains";

    public override string Hints => "This boss rarely appears in craters around the world";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage4;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      ITextureResource icon = null;

      if (IsClient)
        icon = Api.GetProtoEntity<ObjectPragmiumKingRemains>().Icon;

      tasks
           .Add(TaskDestroy.Require<ObjectPragmiumKingRemains>(count: 1).WithIcon(icon));

      prerequisites
           .Add<QuestPragmiumQueenRemains>()
           .Add<QuestSandTyrantRemains>()
           .Add<QuestCookMoreFoodStage4>()
           .Add<QuestMineMoreMineralsStage4>();
    }
  }
}