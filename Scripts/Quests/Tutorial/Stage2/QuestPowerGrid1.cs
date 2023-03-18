using AtomicTorch.CBND.CoreMod.PlayerTasks;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;

namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
  public class QuestPowerGrid1 : ProtoQuest
  {
    public const string HintGridZone =
        "Electrical devices only work when they are [b]within[/b] the land claim zone. Building any electrical devices outside of the land claim zone is pointless.";

    public const string HintPowerStorage =
        "Tier1 electrical energy is stored in land claim buildings.";

    public const string HintUnitedGrid =
        "Several connected land claim zones will behave as a [b]single power grid[/b].";

    public override string Description =>
        "Starting a power grid for your base is an important step toward increased efficiency.";

    public override string Name => "Starting a basic power grid";

    public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
          .Add(TaskBuildStructure.Require<ObjectGeneratorBio>());

      prerequisites
          .Add<QuestBuildAFarm>();

      hints
          .Add(HintGridZone)
          .Add(HintPowerStorage)
          .Add(HintUnitedGrid);
    }
  }
}