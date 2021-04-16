namespace AtomicTorch.CBND.CoreMod.Quests.Book
{
  using AtomicTorch.CBND.CoreMod.Items.Food;
  using AtomicTorch.CBND.CoreMod.PlayerTasks;
  using AtomicTorch.CBND.CoreMod.Quests.Tutorial;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.Scripts.Quests.Base;
  using System.Collections.Generic;

  public class QuestCookMoreFoodStage2 : ProtoQuest
  {
    public override string Description =>
        "Now that you've built a campfire, you can use it to cook some basic food.";

    public override string Name => "Cook more food";

    public override string Hints => "You are always hungry.";

    public override ushort RewardLearningPoints => QuestBookConstants.RewardStage2;

    protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
    {
      var task = new TaskManufactureItem(GetList(), count: 100, description: this.Name);
      
      tasks
          .Add(task);

      prerequisites
          .Add<QuestCookMoreMushrooms>()
          .Add<QuestBuildAFarm>();
    }

    public static List<IProtoItemFood> GetList()
    {
      var list = new List<IProtoItemFood>();
      list.Add(Api.GetProtoEntity<ItemBread>());
      list.Add(Api.GetProtoEntity<ItemCactusDrink>());
      list.Add(Api.GetProtoEntity<ItemCannedBeans>());
      list.Add(Api.GetProtoEntity<ItemCannedFish>());
      list.Add(Api.GetProtoEntity<ItemCannedMeat>());
      list.Add(Api.GetProtoEntity<ItemCannedMixedMeat>());
      list.Add(Api.GetProtoEntity<ItemCarbonara>());
      list.Add(Api.GetProtoEntity<ItemCarrotGrilled>());
      list.Add(Api.GetProtoEntity<ItemCheese>());
      list.Add(Api.GetProtoEntity<ItemChiliBeans>());
      list.Add(Api.GetProtoEntity<ItemChiliPepper>());
      list.Add(Api.GetProtoEntity<ItemCoffeeCup>());
      list.Add(Api.GetProtoEntity<ItemCornGrilled>());
      list.Add(Api.GetProtoEntity<ItemCucumbersPickled>());
      list.Add(Api.GetProtoEntity<ItemMilk>());
      list.Add(Api.GetProtoEntity<ItemMREImprovised>());
      list.Add(Api.GetProtoEntity<ItemOnigiri>());
      list.Add(Api.GetProtoEntity<ItemSaladFruit>());
      list.Add(Api.GetProtoEntity<ItemSaladVegetable>());
      list.Add(Api.GetProtoEntity<ItemSandwich>());
      list.Add(Api.GetProtoEntity<ItemSushi>());
      list.Add(Api.GetProtoEntity<ItemTacoMeat>());
      list.Add(Api.GetProtoEntity<ItemFishDried>());
      list.Add(Api.GetProtoEntity<ItemMeatJerky>());
      list.Add(Api.GetProtoEntity<ItemSalami>());
      list.Add(Api.GetProtoEntity<ItemEggsBoiled>());
      list.Add(Api.GetProtoEntity<ItemEggsFried>());
      list.Add(Api.GetProtoEntity<ItemFishRoasted>());
      list.Add(Api.GetProtoEntity<ItemJamBerries>());
      list.Add(Api.GetProtoEntity<ItemMeatRoasted>());
      list.Add(Api.GetProtoEntity<ItemPieBerry>());
      list.Add(Api.GetProtoEntity<ItemPizzaPineapple>());
      list.Add(Api.GetProtoEntity<ItemPotatoBakedWithMeat>());
      list.Add(Api.GetProtoEntity<ItemPotatoFrenchFries>());
      list.Add(Api.GetProtoEntity<ItemRiceCooked>());
      list.Add(Api.GetProtoEntity<ItemRoastedMushrooms>());
      list.Add(Api.GetProtoEntity<ItemUkha>());
      list.Add(Api.GetProtoEntity<ItemStew>());
      list.Add(Api.GetProtoEntity<ItemYuccaFried>());
      list.Add(Api.GetProtoEntity<ItemMeatCharred>());
      list.Add(Api.GetProtoEntity<ItemYuccaCharred>());
      return list;
    }
  }
}