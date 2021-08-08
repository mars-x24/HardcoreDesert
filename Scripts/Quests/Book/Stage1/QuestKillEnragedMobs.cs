namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestKillEnragedMobs : ProtoQuest
  {
    public override string Description => "Protect your base against enraged mutants.";

    public override string Name => "Protect your base!";

    public override string Hints => "Can be done with the mutant migration event.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage1;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskKill.Require<MobEnragedMutantHyena>(count: 1))
           .Add(TaskKill.Require<MobEnragedMutantBoar>(count: 1))
           .Add(TaskKill.Require<MobEnragedMutantWolf>(count: 1));

      prerequisites
           .Add<QuestBuildAPermanentBase>();
    }
  }
}