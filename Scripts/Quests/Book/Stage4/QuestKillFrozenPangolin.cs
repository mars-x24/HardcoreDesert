using AtomicTorch.CBND.CoreMod.Characters.Mobs;
using AtomicTorch.CBND.CoreMod.PlayerTasks;
using HardcoreDesert.Scripts.Quests.Base;

namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  public class QuestKillFrozenPangolin : ProtoQuest
  {
    public override string Description => "Time for some extreme hunting!";

    public override string Name => "Frozen pangolin";

    public override string Hints => "Can be found in the snow biome. You may need a teleport location data.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage4;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
           .Add(TaskKill.Require<MobFrozenPangolin>(count: 5));

      prerequisites
           .Add<QuestPragmiumKingRemains>();
    }
  }
}