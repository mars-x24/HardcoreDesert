namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
  using AtomicTorch.CBND.CoreMod.CraftRecipes;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Systems.Crafting;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Collections.Generic;

  public class QuestCraftBag : ProtoQuest
  {
    public override string Description =>
        "You may need more inventory space when running around. Having a bag can be useful for storing any medicine or food. Research and craft a bag.";

    public override string Hints =>
        @"[*] Click on this item while holding the Alt key to open it.";

    public override string Name => "Craft a bag";

    public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      tasks
          .Add(TaskHaveTechNode.Require<TechNodeBagSmall>()) 

          .Add(TaskCraftRecipe.RequireStationRecipe(
                                   new List<Recipe.RecipeForStationCrafting>()
                                   {
                                       Api.GetProtoEntity<RecipeBagSmall>(),
                                       Api.GetProtoEntity<RecipeBagLarge>()
                                   }, 1, this.Name));

      prerequisites
          .Add<QuestBuildAPermanentBase>();
    }
  }
}