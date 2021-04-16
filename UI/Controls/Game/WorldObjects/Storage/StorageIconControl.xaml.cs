namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage
{
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
  using System.Windows;

  public partial class StorageIconControl : BaseUserControl
  {
    public static readonly DependencyProperty ItemStorageProperty =
        DependencyProperty.Register("ItemStorage",
                                    typeof(IItem),
                                    typeof(StorageIconControl),
                                    new PropertyMetadata(default(IItem)));

    private FrameworkElement layoutRoot;

    private ViewModelStorageIconControl viewModel;

    public IItem ItemStorage
    {
      get => this.GetValue(ItemStorageProperty) as IItem;
      set => this.SetValue(ItemStorageProperty, value);
    }

    protected override void InitControl()
    {
      this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
    }

    protected override void OnLoaded()
    {
      if (this.ItemStorage is null)
      {
        return;
      }
      
      this.RefreshViewModel();
    }

    public void RefreshViewModel()
    {
      if(this.layoutRoot.DataContext is not null)
      {
        this.layoutRoot.DataContext = null;
        this.viewModel?.Dispose();
        this.viewModel = null;
      }

      this.layoutRoot.DataContext = this.viewModel = new ViewModelStorageIconControl(this.ItemStorage);
    }

    protected override void OnUnloaded()
    {
      this.layoutRoot.DataContext = null;
      this.viewModel?.Dispose();
      this.viewModel = null;
    }
  }
}