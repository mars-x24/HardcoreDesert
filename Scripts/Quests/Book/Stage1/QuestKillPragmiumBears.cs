namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestKillPragmiumBears : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting!";

    public override string Name => "Pragmium bears hunting.";

    public override string Hints => "Each animal has different loot you can expect to find.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage1;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskKill.Require<MobPragmiumBear>(count: 5))
           .Add(TaskKill.Require<MobLargePragmiumBear>(count: 1));

      prerequisites
           .Add<QuestMasterHunter6>();
    }
  }
}