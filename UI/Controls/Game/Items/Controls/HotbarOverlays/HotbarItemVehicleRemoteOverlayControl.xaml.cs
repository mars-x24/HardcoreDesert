namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
  using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
  using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.VehicleRemoteControl;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

  public partial class HotbarItemVechicleRemoteOverlayControl : BaseUserControl
  {
    private readonly IItem item;

    private ViewModelHotbarItemVehicleRemoteOverlayControl viewModel;

    public HotbarItemVechicleRemoteOverlayControl()
    {

    }

    public HotbarItemVechicleRemoteOverlayControl(IItem item)
    {
      this.item = item;
    }

    protected override void InitControl()
    {
      this.MouseLeftButtonUp += HotbarItemVechicleRemoteOverlayControl_MouseLeftButtonUp;

      this.DataContext = this.viewModel = new ViewModelHotbarItemVehicleRemoteOverlayControl()
      {
        Item = this.item
      };
    }

    private void HotbarItemVechicleRemoteOverlayControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      WindowVehicleRemoteControl.Open(this.item);

      ClientInteractionUISystem.Register(
      Api.Client.Characters.CurrentPlayerCharacter,
      WindowVehicleRemoteControl.instance,
      onMenuClosedByClient:
      () =>
      {
        this.viewModel.UpdateIcon();
      });

    }

    protected override void OnUnloaded()
    {
      this.DataContext = null;
      if (this.viewModel is not null)
        this.viewModel.Dispose();
      this.viewModel = null;
    }
  }
}