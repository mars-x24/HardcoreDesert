namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestKillColdBear : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting!";

    public override string Name => "Cold bear";

    public override string Hints => "This monster does not like any of the known biomes. You may need a teleport location data.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage4;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskKill.Require<MobColdBear>(count: 5));

      prerequisites
           .Add<QuestPragmiumKingRemains>();
    }
  }
}