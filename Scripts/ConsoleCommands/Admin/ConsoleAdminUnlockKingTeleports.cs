// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.Console;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Scripting;

  public class ConsoleAdminUnlockKingTeleports : BaseConsoleCommand
  {
    public override string Description =>
        "Unlock King's teleports";

    public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

    public override string Name => "admin.unlockKingTeleports";

    public string Execute()
    {
      return MobBossPragmiumKing.SpawnTeleport(null).ToString();
    }


  }
}