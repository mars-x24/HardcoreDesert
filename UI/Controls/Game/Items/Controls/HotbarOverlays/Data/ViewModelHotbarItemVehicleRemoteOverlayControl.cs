namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data
{
  using AtomicTorch.CBND.CoreMod.Helpers.Client;
  using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
  using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Windows.Media;

  public class ViewModelHotbarItemVehicleRemoteOverlayControl : BaseViewModel
  {
    private VehicleRemoteActionState currentAction;

    private IItem item;

    public ViewModelHotbarItemVehicleRemoteOverlayControl()
    {
      var characterState = ClientCurrentCharacterHelper.PrivateState;
      characterState.ClientSubscribe(
          _ => _.CurrentActionState,
          s => { this.CurrentAction = s as VehicleRemoteActionState; },
          this);

      this.CurrentAction = characterState.CurrentActionState as VehicleRemoteActionState;
    }

    public IItem Item
    {
      get => this.item;
      set
      {
        if (this.item == value)
        {
          return;
        }

        if (this.item is not null)
        {
          this.ReleaseSubscriptions();
        }

        this.item = value;

        this.UpdateIcon();
      }
    }

    public double TimeDurationSeconds { get; private set; }

    public Brush VehicleIcon { get; private set; }

    private VehicleRemoteActionState CurrentAction
    {
      get => this.currentAction;
      set
      {
        if (this.currentAction == value)
        {
          return;
        }

        if (value is null
            || value.ItemVehicle != this.item)
        {
          this.TimeDurationSeconds = 0;
          return;
        }

        this.currentAction = value;
        this.TimeDurationSeconds = this.currentAction.TimeRemainsSeconds;

        this.UpdateIcon();
      }
    }

    public void UpdateIcon()
    {
      Brush icon = Api.Client.UI.GetTextureBrush(new TextureResource("Icons/MapExtras/VehicleHoverboard.png"));

      if (this.item is not null)
      {
        var itemPrivateState = this.item.GetPrivateState<ItemVehicleRemoteControlPrivateState>();

        if (itemPrivateState.VehicleProto is not null)
          icon = Api.Client.UI.GetTextureBrush(itemPrivateState.VehicleProto.Icon);
      }

      this.VehicleIcon = icon;
    }
  }
}