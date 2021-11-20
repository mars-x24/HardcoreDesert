namespace HardcoreDesert.UI.Controls.Game.WorldObjects.Robot
{
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.UI.Controls.Game.WorldObjects.Robot.Data;

  public partial class WindowItemRobot : BaseUserControlWithWindow
  {
    public static WindowItemRobot instance;

    private ViewModelWindowItemRobot viewModel;

    private IItem itemRobot;

    public static void Open(IItem itemRobot)
    {
      if (instance?.IsOpened == true && instance.itemRobot == itemRobot)
      {
        instance.CloseWindow();
      }
      else
      {
        if (instance == null)
        {
          instance = new WindowItemRobot();
          instance.itemRobot = itemRobot;
          Api.Client.UI.LayoutRootChildren.Add(instance);
        }
        else
        {
          instance.itemRobot = itemRobot;
          instance.RefreshViewModel();
        }

        ClientCurrentInteractionMenu.RegisterMenuWindow(instance);
        ClientCurrentInteractionMenu.Open();
      }
    }

    public static void Close(IItem itemRobot)
    {
      if (instance?.IsOpened == true && instance.itemRobot == itemRobot)
      {
        instance.CloseWindow();
      }
    }

    protected override void InitControlWithWindow()
    {
      this.Window.IsCached = false;
    }

    protected override void OnLoaded()
    {
      base.OnLoaded();

      this.RefreshViewModel();
    }

    public void RefreshViewModel()
    {
      if (DataContext is not null)
      {
        this.DataContext = null;
        this.viewModel.Dispose();
        this.viewModel = null;
      }

      this.DataContext = this.viewModel = new ViewModelWindowItemRobot(this.itemRobot);
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