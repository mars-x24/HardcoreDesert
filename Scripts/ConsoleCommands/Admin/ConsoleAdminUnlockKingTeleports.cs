// ReSharper disable CanExtractXamlLocalizableStringCSharp

using AtomicTorch.CBND.CoreMod.Characters.Mobs;
using AtomicTorch.CBND.CoreMod.Systems.Console;
using AtomicTorch.CBND.GameApi.Scripting;

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
  public class ConsoleAdminUnlockKingTeleports : BaseConsoleCommand
  {
    public override string Description =>
        "Unlock King's teleports";

    public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

    public override string Name => "admin.unlockKingTeleports";

    public string Execute()
    {
      using var playerCharacters = Api.Shared.WrapInTempList(
        Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false));

      return MobBossPragmiumKing.SpawnTeleport(playerCharacters.AsList()).ToString();
    }


  }
}