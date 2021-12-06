// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Console;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Scripting;

  public class ConsoleAdminClearLandClaimGroups : BaseConsoleCommand
  {
    public override string Description =>
        "Clear the list of land claim groups for global storage.";

    public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

    public override string Name => "admin.clearLandClaimGroups";

    public string Execute()
    {
      using var tempList = Api.Shared.GetTempList<ILogicObject>();
      Api.GetProtoEntity<LandClaimGroup>()
         .GetAllGameObjects(tempList.AsList());

      foreach (var group in tempList.AsList())
      {
        Api.Server.World.DestroyObject(group);
      }

      return "Ok";
    }


  }
}