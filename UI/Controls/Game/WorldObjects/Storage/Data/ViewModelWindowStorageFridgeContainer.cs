namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Storage.Data
{
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Items.Storage;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using System;

  public class ViewModelWindowStorageFridgeContainer : ViewModelWindowStorageContainer
  {
    private ItemStorageFridgePrivateState privateState;
    private ItemStorageFridgePublicState publicState;

    public ViewModelWindowStorageFridgeContainer(IItem itemStorage) : base(itemStorage)
    {
      this.privateState = itemStorage.GetPrivateState<ItemStorageFridgePrivateState>();
      this.publicState = itemStorage.GetPublicState<ItemStorageFridgePublicState>();

      var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
      ClientContainersExchangeManager.Register(
          this,
          this.ItemsEnergyContainer,
          allowedTargets: new[]
          {
                            currentCharacter.SharedGetPlayerContainerInventory(),
                            currentCharacter.SharedGetPlayerContainerHotbar()
          });

      this.isOn = this.publicState.IsOn;

      this.publicState.ClientSubscribe(
          _ => _.IsOn,
          _ => 
          {
            this.isOn = this.publicState.IsOn;
            this.NotifyPropertyChanged("IsOn");
            this.NotifyPropertyChanged("IsOff");
          },
          this);
    }

    protected override void DisposeViewModel()
    {
      base.DisposeViewModel();
    }

    public IClientItemsContainer ItemsEnergyContainer => (IClientItemsContainer)this.privateState.ItemsEnergyContainer;

    public bool IsOn
    {
      get => this.isOn;
      set
      {
        this.isOn = value;
        ((ProtoItemStorageFridge)this.ItemStorage.ProtoItem).CallServer(_ => _.ServerRemote_ApplyClientIsOn(this.ItemStorage, value));
      }
    }

    public bool IsOff
    {
      get => !this.isOn;
    }

    private bool isOn;


    public string PerishableItemsStorageDurationText
    {
      get
      {
        var protoFridge = (ProtoItemStorageFridge)this.ItemStorage.ProtoGameObject;
        var resultMult = protoFridge.FreshnessDurationMultiplier;

        var resultText = Math.Round(resultMult,
                                    digits: 2,
                                    MidpointRounding.AwayFromZero)
                             .ToString("0.##");

        resultText = string.Format(ProtoItemStorageFridge.HintFreshness, resultText);
        return resultText;
      }
    }

  }
}