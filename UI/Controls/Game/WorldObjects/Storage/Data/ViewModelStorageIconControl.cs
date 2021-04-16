namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage.Data
{
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

  public class ViewModelStorageIconControl : BaseViewModel
  {
    private static readonly ITextureResource TextureResourcePlaceholderIcon
        = new TextureResource("StaticObjects/Structures/Crates/ObjectCrate_PlateIcon");

    private readonly ItemStoragePublicState publicState;

    private readonly IItem itemStorage;

    public ViewModelStorageIconControl(IItem itemStorage)
    {
      this.itemStorage = itemStorage;
      this.publicState = itemStorage.GetPublicState<ItemStoragePublicState>();
      this.publicState.ClientSubscribe(_ => _.IconSource,
                                       _ =>
                                       {
                                         this.NotifyPropertyChanged(nameof(this.IsIconAvailable));
                                         this.NotifyPropertyChanged(nameof(this.Icon));
                                       },
                                       this);
    }

    public BaseCommand CommandResetIcon => new ActionCommand(this.ExecuteCommandResetIcon);

    public BaseCommand CommandSelectIcon => new ActionCommand(this.ExecuteCommandSelectIcon);

    public TextureBrush Icon
    {
      get
      {
        var icon = ClientCrateIconHelper.GetIcon(this.publicState.IconSource);
        return icon is not null
               && !TextureResource.NoTexture.Equals(icon)
                   ? Api.Client.UI.GetTextureBrush(icon)
                   : null;
      }
    }

    public TextureBrush IconPlaceholder => Api.Client.UI.GetTextureBrush(TextureResourcePlaceholderIcon);

    public bool IsIconAvailable => this.publicState.IconSource is not null;

    protected override void DisposeViewModel()
    {
      base.DisposeViewModel();
    }

    private void ExecuteCommandResetIcon()
    {
      var protoObject = (IProtoItemStorage)this.itemStorage.ProtoGameObject;
      protoObject.ClientSetIconSource(this.itemStorage, null);
    }

    private void ExecuteCommandSelectIcon()
    {
      var protoObjectStorage = (IProtoItemStorage)this.itemStorage.ProtoGameObject;
      var protoItemInHand = ClientItemsManager.ItemInHand?.ProtoItem;
      protoObjectStorage.ClientSetIconSource(this.itemStorage, protoItemInHand);
    }
  }
}