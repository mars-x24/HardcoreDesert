namespace HardcoreDesert.UI.Controls.Game.WorldObjects.Robot.Data
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using HardcoreDesert.Scripts.Systems.Robot;

  public class ViewModelRobotState : BaseViewModel
  {
    private readonly LandClaimAreaPrivateState state;
    private readonly IStaticWorldObject landClaim;

    public ViewModelRobotState(LandClaimAreaPrivateState state, IStaticWorldObject landClaim)
    {
      this.state = state;
      this.landClaim = landClaim;

      state.ClientSubscribe(
          _ => _.RobotManufacturerInputEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerInputSlots)),
          this);

      state.ClientSubscribe(
          _ => _.RobotManufacturerOutputEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerOutputSlots)),
          this);

      state.ClientSubscribe(
          _ => _.RobotManufacturerCharacterInventoryEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerCharacterInventory)),
          this);

      state.ClientSubscribe(
          _ => _.RobotManufacturerEnderCrateEnabled,
          _ => this.NotifyPropertyChanged(nameof(this.ManufacturerEnderCrate)),
          this);
    }

    public bool IsRobotsAvailableForCurrentTier =>
      this.landClaim.ProtoGameObject is not ObjectLandClaimT1 &&
       this.landClaim.ProtoGameObject is not ObjectLandClaimT2;

    public bool ManufacturerInputSlots
    {
      get { return this.state.RobotManufacturerInputEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerSettings(state.GameObject as ILogicObject, Api.Client.Characters.CurrentPlayerCharacter.Name, true, value); }
    }

    public bool ManufacturerOutputSlots
    {
      get { return this.state.RobotManufacturerOutputEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerSettings(state.GameObject as ILogicObject, Api.Client.Characters.CurrentPlayerCharacter.Name, false, value); }
    }

    public bool ManufacturerCharacterInventory
    {
      get { return this.state.RobotManufacturerCharacterInventoryEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerCharacterInventorySetting(state.GameObject as ILogicObject, Api.Client.Characters.CurrentPlayerCharacter.Name, value); }
    }

    public bool ManufacturerEnderCrate
    {
      get { return this.state.RobotManufacturerEnderCrateEnabled; }
      set { RobotSystem.ClientSetRobotManufacturerEnderCrateSetting(state.GameObject as ILogicObject, Api.Client.Characters.CurrentPlayerCharacter.Name, value); }
    }

  }
}