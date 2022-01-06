namespace AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners
{
  using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
  using AtomicTorch.CBND.CoreMod.Helpers.Server;
  using AtomicTorch.CBND.CoreMod.Systems.Construction;
  using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

  public class ClientComponentVehicleBackupWeapon : ClientComponent
  {
    public override void Update(double deltaTime)
    {
      if (ClientInputManager.IsButtonDown(GameButton.ActionInteract, true))
      {
        VehicleBackupWeaponSystem.ClientTrySwitchingWeapon();
      }
    }
  }
}