namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

  public partial class WindowStorageFridgeContainer : BaseUserControlWithWindow
  {
    public static WindowStorageFridgeContainer instance;

    private ViewModelWindowStorageContainer viewModel;

    private StorageIconControl iconControl;

    private IItem itemStorage;

    public static void Open(IItem itemStorage)
    {
      if (instance?.IsOpened == true && instance.itemStorage == itemStorage)
      {
        instance.CloseWindow();
      }
      else
      {
        if (instance == null)
        {
          instance = new WindowStorageFridgeContainer();
          instance.itemStorage = itemStorage;
          Api.Client.UI.LayoutRootChildren.Add(instance);
        }
        else
        {
          instance.itemStorage = itemStorage;
          instance.RefreshViewModel();
        }

        ClientCurrentInteractionMenu.RegisterMenuWindow(instance);
        ClientCurrentInteractionMenu.Open();
      }
    }

    protected override void InitControlWithWindow()
    {
      this.iconControl = this.GetByName<StorageIconControl>("IconControl");

      this.Window.IsCached = false;
    }

    protected override void OnLoaded()
    {
      base.OnLoaded();

      this.RefreshViewModel();
    }

    public void RefreshViewModel()
    {
      if(DataContext is not null)
      {
        this.DataContext = null;
        this.viewModel.Dispose();
        this.viewModel = null;
      } 

      this.DataContext = this.viewModel = new ViewModelWindowStorageFridgeContainer(this.itemStorage);

      this.iconControl.RefreshViewModel();
    }

    protected override void OnUnloaded()
    {
      base.OnUnloaded();

      this.DataContext = null;
      this.viewModel.Dispose();
      this.viewModel = null;
      instance = null;
    }


  }
}