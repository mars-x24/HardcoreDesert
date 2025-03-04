﻿using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
using AtomicTorch.CBND.CoreMod.Systems.VehicleNamesSystem;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
using AtomicTorch.GameEngine.Common.Extensions;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.VehicleRemoteControl.Data
{
  public class ViewModelVehicleRemoteControlEntry : BaseViewModel
  {
    private VehicleStatus status;

    public ViewModelVehicleRemoteControlEntry(
        uint vehicleGameObjectId,
        IProtoVehicle protoVehicle,
        VehicleStatus status)
    {
      this.VehicleGameObjectId = vehicleGameObjectId;
      this.ProtoVehicle = protoVehicle;
      this.Status = status;

      VehicleNamesSystem.ClientVehicleNameChanged += this.VehicleNameChangedHandler;
    }

    public TextureBrush Icon => Api.Client.UI.GetTextureBrush(this.ProtoVehicle.Icon);

    public IProtoVehicle ProtoVehicle { get; }

    public VehicleStatus Status
    {
      get => this.status;
      set
      {
        if (this.status == value)
        {
          return;
        }

        this.status = value;
        this.NotifyThisPropertyChanged();
        this.NotifyPropertyChanged(nameof(this.StatusText));
      }
    }

    public string StatusText => this.Status.GetDescription();

    public string Title
        => VehicleNamesSystem.ClientTryGetVehicleName(this.VehicleGameObjectId)
           ?? this.ProtoVehicle.Name;

    public uint VehicleGameObjectId { get; }

    protected override void DisposeViewModel()
    {
      VehicleNamesSystem.ClientVehicleNameChanged -= this.VehicleNameChangedHandler;
      base.DisposeViewModel();
    }

    private void VehicleNameChangedHandler(uint vehicleid, string vehiclename)
    {
      if (vehicleid == this.VehicleGameObjectId)
      {
        this.NotifyPropertyChanged(nameof(this.Title));
      }
    }
  }
}