// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Console;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Scripting;

  public class ConsoleAdminCleanLandClaimGroups : BaseConsoleCommand
  {
    public override string Description =>
        "Clean the list of land claim groups for global storage.";

    public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

    public override string Name => "admin.cleanLandClaimGroups";

    public string Execute()
    {
      using var tempList = Api.Shared.GetTempList<ILogicObject>();
      Api.GetProtoEntity<LandClaimGroup>()
         .GetAllGameObjects(tempList.AsList());

      foreach (var group in tempList.AsList())
      {
        var privateState = LandClaimGroup.GetPrivateState(group);
        if (privateState.ItemsContainerGlobalStorage is not null && privateState.ItemsContainerGlobalStorage.OccupiedSlotsCount == 0)
        {
          Api.Server.World.DestroyObject(group);
          continue;
        }

        for(int i = privateState.ServerLandClaimAreasGroups.Count - 1; i > 0;i--)
        {
          privateState.ServerLandClaimAreasGroups.RemoveAt(i);
        }
      }

      return "Ok";
    }


  }
}