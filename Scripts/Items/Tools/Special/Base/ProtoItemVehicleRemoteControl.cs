namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
  using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
  using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.VehicleRemoteControl;
  using AtomicTorch.CBND.CoreMod.Vehicles;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System.Windows.Controls;

  public abstract class ProtoItemVehicleRemoteControl
      : ProtoItemTool<ItemVehicleRemoteControlPrivateState,
            EmptyPublicState,
            EmptyClientState>,
        IProtoItemVehicleRemoteControl
  {
    public override bool CanBeSelectedInVehicle => true;

    public double ActionDuratioRecallSeconds => 20;


    public double EngeryUse => 2000;

    public Control ClientCreateHotbarOverlayControl(IItem item)
    {
      return new HotbarItemVechicleRemoteOverlayControl(item);
    }

    public override void ServerItemHotbarSelectionChanged(
        IItem item,
        ICharacter character,
        bool isSelected)
    {

      if (isSelected)
      {

      }
    }

    //public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
    //{
    //  if (!base.SharedCanSelect(item, character, isAlreadySelected, isByPlayer))
    //  {
    //    return false;
    //  }

    //  if (CharacterEnergySystem.SharedHasEnergyCharge(character, this.EngeryUse))
    //  {
    //    return true;
    //  }

    //  // cannot select
    //  if (IsClient && isByPlayer)
    //  {
    //    CharacterEnergySystem.ClientShowNotificationNotEnoughEnergyCharge(this);
    //  }

    //  return false;
    //}


    protected override bool ClientItemUseFinish(ClientItemData data)
    {
      VehicleRemoteSystem.Instance.ClientTryAbortAction();
      return false;
    }

    protected override void ClientItemUseStart(ClientItemData data)
    {
      if (data.PrivateState.VehicleID == 0)
      {
        WindowVehicleRemoteControl.Open(data.Item);
      }
      else
      {
        var character = Api.Client.Characters.CurrentPlayerCharacter;
        if (!CharacterEnergySystem.SharedHasEnergyCharge(character, this.EngeryUse))
        {
          CharacterEnergySystem.ClientShowNotificationNotEnoughEnergyCharge(this);
          return;
        }

        VehicleRemoteSystem.Instance.ClientTryStartAction();
      }
    }
  }


  public class ItemVehicleRemoteControlPrivateState : ItemWithDurabilityPrivateState
  {
    [SyncToClient(deliveryMode: DeliveryMode.ReliableSequenced)]
    public uint VehicleID { get; set; }

    [SyncToClient(deliveryMode: DeliveryMode.Default)]
    public IProtoVehicle VehicleProto { get; set; }
  }

}