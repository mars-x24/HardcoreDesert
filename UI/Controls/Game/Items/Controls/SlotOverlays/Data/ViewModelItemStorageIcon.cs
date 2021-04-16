namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays.Data
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Windows.Media;

  public class ViewModelItemStorageIcon : BaseViewModel
  {
    private ItemStoragePublicState publicState;

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

        this.publicState = this.item.GetPublicState<ItemStoragePublicState>();
        this.publicState.ClientSubscribe(_ => _.IconSource,
                                                    _ => this.Refresh(),
                                                    this);

        this.Refresh();
      }
    }

    private void Refresh()
    {
      var icon = ClientCrateIconHelper.GetIcon(this.publicState.IconSource);
      this.Brush = Api.Client.UI.GetTextureBrush(icon);
    }

  }
}