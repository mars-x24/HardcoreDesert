namespace CryoFall.HardcoreDesert
{
  using AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners;
  using AtomicTorch.CBND.GameApi.Scripting;


  public class BootstrapperHardcoreDesert : BaseBootstrapper
  {
    private static IClientSceneObject sceneObjectInputComponents;

    private static bool isInitialized;

    public override void ClientInitialize()
    {
      Reset();

      isInitialized = true;

      var input = Api.Client.Scene.CreateSceneObject("Input hardcore desert components");
      input.AddComponent<ClientComponentVehicleBackupWeapon>();

      sceneObjectInputComponents = input;
    }

    private static void Reset()
    {
      if (!isInitialized)
      {
        return;
      }

      isInitialized = false;

      sceneObjectInputComponents?.Destroy();
      sceneObjectInputComponents = null;
    }
  }
}