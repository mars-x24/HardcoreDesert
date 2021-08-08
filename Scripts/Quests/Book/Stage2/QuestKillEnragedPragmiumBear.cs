namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestKillEnragedPragmiumBear : ProtoQuest
  {
    public override string Description => "Protect your base against an enraged pragmium bear";

    public override string Name => "Enraged Pragmium Bear";

    public override string Hints => "Can be done with the mutant migration event.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage2;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskKill.Require<MobEnragedPragmiumBear>(count: 1));

      prerequisites
           .Add<QuestKillEnragedMobs>();
    }
  }
}