namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.VehicleRemoteControl
{
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.VehicleRemoteControl.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Scripting;

  public partial class WindowVehicleRemoteControl : BaseUserControlWithWindow
  {
    public static WindowVehicleRemoteControl instance;

    private IItem remote;

    public ViewModelVehicleRemoteControl ViewModel { get; private set; }

    public static void CloseActiveMenu()
    {
      instance?.CloseWindow();
    }

    public static void Open(IItem remote)
    {
      if (instance?.IsOpened == true)
      {
        instance.CloseWindow();
      }
      else
      {
        if (instance == null)
        {
          instance = new WindowVehicleRemoteControl();
          instance.remote = remote;
          Api.Client.UI.LayoutRootChildren.Add(instance);
        }

        ClientCurrentInteractionMenu.RegisterMenuWindow(instance);
        ClientCurrentInteractionMenu.Open();
      }
    }


    protected override void InitControlWithWindow()
    {
      this.Window.IsCached = false;
    }

    protected override void OnLoaded()
    {
      base.OnLoaded();

      this.DataContext = this.ViewModel = new Data.ViewModelVehicleRemoteControl(this.remote);
    }

    protected override void OnUnloaded()
    {
      base.OnUnloaded();

      this.DataContext = null;
      this.ViewModel.Dispose();
      this.ViewModel = null;
      instance = null;
    }
  }
}