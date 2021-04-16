namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using HardcoreDesert.Scripts.Quests.Base;

  public class QuestKillAngryPangolins : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting!";

    public override string Name => "Angry Pangolins hunting.";

    public override string Hints => "Watch out, this guy is toxic.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage1;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskKill.Require<MobAngryPangolin>(count: 10));

      prerequisites
           .Add<QuestMasterHunter6>();
    }
  }
}