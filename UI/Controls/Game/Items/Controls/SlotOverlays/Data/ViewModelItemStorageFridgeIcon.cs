namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Windows.Media;

  public class ViewModelItemStorageFridgeIcon : BaseViewModel
  {
    private ItemStorageFridgePublicState publicState;

    private IItem item;

    public Brush Brush { get; private set; }

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
        if (this.item is null)
        {
          return;
        }

        this.publicState = this.item.GetPublicState<ItemStorageFridgePublicState>();
        this.publicState.ClientSubscribe(_ => _.IconOverlay,
                                                    _ => this.Refresh(),
                                                    this);

        this.Refresh();
      }
    }

    private void Refresh()
    {
      this.Brush = null;
      if (!string.IsNullOrEmpty(this.publicState.IconOverlay))
      {
        var icon = new TextureResource(publicState.IconOverlay);
        this.Brush = Api.Client.UI.GetTextureBrush(icon);
      }
    }

  }
}