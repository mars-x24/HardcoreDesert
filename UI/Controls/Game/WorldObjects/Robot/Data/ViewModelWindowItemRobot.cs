using AtomicTorch.CBND.CoreMod.Items.Robots;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Scripting;
using HardcoreDesert.Scripts.Systems.Robot;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HardcoreDesert.UI.Controls.Game.WorldObjects.Robot.Data
{
  public class ViewModelWindowItemRobot : BaseViewModel
  {
    private readonly IItem itemRobot;

    private ItemRobotPrivateState state;

    public ViewModelItemTooltip ViewModelItemTooltip { get; private set; }

    public ObservableCollection<ViewModelManufacturerEntity> EntityCollection { get; private set; }

    public ViewModelWindowItemRobot(IItem itemRobot)
    {
      this.EntityCollection = new ObservableCollection<ViewModelManufacturerEntity>();
      var listStructures = new List<IProtoObjectStructure>();
      listStructures.AddRange(Api.FindProtoEntities<IProtoObjectManufacturer>());
      listStructures.AddRange(Api.FindProtoEntities<IProtoObjectSprinkler>());

      foreach (var entity in listStructures)
      {
        if (entity is ProtoObjectBarrel)
          continue;

        var viewModelEntity = new ViewModelManufacturerEntity(entity);
        viewModelEntity.IsEnabledChanged += ViewModelEntity_IsEnabledChanged;
        this.EntityCollection.Add(viewModelEntity);
      }

      this.itemRobot = itemRobot;

      this.state = itemRobot.GetPrivateState<ItemRobotPrivateState>();

      this.state.ClientSubscribe(
          _ => _.RobotManufacturerInputEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerInputSlots)),
          this);

      this.state.ClientSubscribe(
          _ => _.RobotManufacturerOutputEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerOutputSlots)),
          this);

      this.state.ClientSubscribe(
          _ => _.RobotManufacturerFuelEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerFuelSlots)),
          this);

      this.state.ClientSubscribe(
          _ => _.TimeRunIntervalSeconds,
          _ => this.NotifyPropertyChanged(nameof(this.TimeRunIntervalSeconds)),
          this);

      this.state.ClientSubscribe(
          _ => _.StructureLoadPercent,
          _ => this.NotifyPropertyChanged(nameof(this.StructureLoadPercent)),
          this);

      this.state.ClientSubscribe(
        _ => _.LoadInactiveOnly,
        _ => this.NotifyPropertyChanged(nameof(this.ManufacturerLoadInactiveOnly)),
        this);

      this.state.ClientSubscribe(
          _ => _.AllowedStructures,
          _ => this.LoadAllowedStructure(),
          this);

      this.LoadAllowedStructure();

      this.ViewModelItemTooltip = new ViewModelItemTooltip(this.itemRobot, this.itemRobot.ProtoItem);
    }

    public string RobotName => "Robot ID : " + this.itemRobot.Id.ToString();

    public bool ManufacturerInputSlots
    {
      get { return this.state.RobotManufacturerInputEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerSlotSetting(this.state.GameObject as IItem, true, value); }
    }

    public bool ManufacturerOutputSlots
    {
      get { return this.state.RobotManufacturerOutputEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerSlotSetting(this.state.GameObject as IItem, false, value); }
    }

    public bool ManufacturerFuelSlots
    {
      get { return this.state.RobotManufacturerFuelEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerFuelSetting(this.state.GameObject as IItem, value); }
    }

    public ushort TimeRunIntervalSeconds
    {
      get { return this.state.TimeRunIntervalSeconds; }
      set { RobotSystem.ClientSetRobotManufacturerTimeRunSetting(this.state.GameObject as IItem, value); }
    }

    public byte StructureLoadPercent
    {
      get { return this.state.StructureLoadPercent; }
      set { RobotSystem.ClientSetRobotManufacturerLoadSetting(state.GameObject as IItem, value); }
    }

    public bool ManufacturerLoadInactiveOnly
    {
      get { return this.state.LoadInactiveOnly; }
      set { RobotSystem.ClientSetRobotManufacturerLoadInactiveOnlySetting(state.GameObject as IItem, value); }
    }




    public void LoadAllowedStructure()
    {
      foreach (var entity in this.EntityCollection)
      {
        bool enabled = this.state.AllowedStructures != null && this.state.AllowedStructures.Contains(entity.Entity);
        entity.Load(enabled);
      }
    }

    private void ViewModelEntity_IsEnabledChanged(ViewModelManufacturerEntity obj)
    {
      RobotSystem.ClientSetRobotManufacturerStructureSetting(this.state.GameObject as IItem, obj.Entity, obj.IsEnabled);
    }

    protected override void DisposeViewModel()
    {
      base.DisposeViewModel();

      this.ViewModelItemTooltip.Dispose();
      this.ViewModelItemTooltip = null;

      foreach (var viewModelEntity in this.EntityCollection)
      {
        viewModelEntity.Dispose();
      }
      this.EntityCollection.Clear();
      this.EntityCollection = null;
    }

  }
}