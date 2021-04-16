namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays
{
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

  public partial class ItemSlotStorageIconOverlayControl : BaseUserControl
    {
        private IItem item;

        private ViewModelItemStorageIcon viewModel;

        public static ItemSlotStorageIconOverlayControl Create(IItem item)
        {
            return new() { item = item };
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelItemStorageIcon()
            {
                Item = this.item
            };
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}