namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.VehicleRemoteControl.Data
{
  using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
  using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
  using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Threading.Tasks;

  public class ViewModelVehicleRemoteControl : BaseViewModel
  {
    private IItem item;

    private ViewModelVehicleRemoteControlEntry selectedVehicle;

    public ViewModelVehicleRemoteControl(IItem item)
    {
      this.item = item;
      this.Refresh();
    }

    public ObservableCollection<ViewModelVehicleRemoteControlEntry> AccessibleVehicles { get; }
        = new SuperObservableCollection<ViewModelVehicleRemoteControlEntry>();

    public BaseCommand CommandSelectVehicle => new ActionCommand(this.ExecuteCommandSelectVehicle);

    public BaseCommand CommandCancelVehicle => new ActionCommand(this.ExecuteCommandCancelVehicle);

    public ViewModelVehicleRemoteControlEntry SelectedVehicle
    {
      get => this.selectedVehicle;
      set
      {
        if (this.selectedVehicle == value)
        {
          return;
        }

        this.selectedVehicle = value;
        this.NotifyThisPropertyChanged();
      }
    }

    private async void Refresh()
    {
      if (this.IsDisposed)
        return;

      ClientTimersSystem.AddAction(delaySeconds: 1.0,
                                   this.Refresh);

      var currentVehicles = await VehicleRemoteSystem.ClientGetVehiclesListAsync();
      if (this.IsDisposed)
        return;

      this.ApplyCurrentVehicles(currentVehicles);
    }

    private void ApplyCurrentVehicles(IReadOnlyList<GarageVehicleEntry> currentVehicles)
    {
      // remove all view models which are not existing in the current vehicles list
      for (var index = 0; index < this.AccessibleVehicles.Count; index++)
      {
        var viewModel = this.AccessibleVehicles[index];
        var isFoundInCurrentEntries = false;
        foreach (var entry in currentVehicles)
        {
          if (viewModel.VehicleGameObjectId != entry.Id)
          {
            continue;
          }

          isFoundInCurrentEntries = true;
          break;
        }

        if (isFoundInCurrentEntries)
        {
          continue;
        }

        // this view model has no corresponding vehicle entry anymore
        this.AccessibleVehicles.RemoveAt(index--);
        viewModel.Dispose();
      }

      // update status or create view models for current vehicles
      foreach (var entry in currentVehicles)
      {
        var isFound = false;
        foreach (var viewModel in this.AccessibleVehicles)
        {
          if (viewModel.VehicleGameObjectId != entry.Id)
          {
            continue;
          }

          isFound = true;
          viewModel.Status = entry.Status;
          break;
        }

        if (!isFound)
        {
          this.AccessibleVehicles.Add(
              new ViewModelVehicleRemoteControlEntry(entry.Id, entry.ProtoVehicle, entry.Status));
        }
      }

      this.SelectedVehicle ??= this.AccessibleVehicles?.FirstOrDefault();
    }

    private async void ExecuteCommandSelectVehicle()
    {
      await VehicleRemoteSystem.Instance.CallServer(_ => _.ServerRemote_UpdateVehicleId(this.item, this.selectedVehicle.VehicleGameObjectId, false));

      WindowVehicleRemoteControl.CloseActiveMenu();
    }

    private async void ExecuteCommandCancelVehicle()
    {
      await VehicleRemoteSystem.Instance.CallServer(_ => _.ServerRemote_UpdateVehicleId(this.item, this.selectedVehicle.VehicleGameObjectId, true));

      WindowVehicleRemoteControl.CloseActiveMenu();
    }


  }
}