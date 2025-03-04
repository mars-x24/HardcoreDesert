﻿using AtomicTorch.CBND.CoreMod.Helpers.Client;
using AtomicTorch.CBND.CoreMod.Rates;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
using AtomicTorch.CBND.CoreMod.Systems.Creative;
using AtomicTorch.CBND.CoreMod.Systems.Faction;
using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ShieldProtection.Data;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
using AtomicTorch.GameEngine.Common.Extensions;
using HardcoreDesert.UI.Controls.Game.WorldObjects.Robot.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims.Data
{
  public class ViewModelWindowLandClaim : BaseViewModel
  {
    public const string AccessListEmpty = "You can provide other survivors with access to your land claim.";

    public const string AccessListTitle = "Access";

    public const string DecayInfoDemoVersion =
        @"Please note: This land claim was [b]founded by a demo version player[/b].
              [br]Because of this, the delay before the decay starts as well as the decay duration itself are [b]shortened[/b].
              [br]When a demo player purchases the game, those durations revert to their standard values.";

    public const string DecayInfoFormat =
        @"Abandoned structures will start decaying after some time
              [br]and eventually disappear to prevent cluttering the map.
              [br]
              [br]This land claim building will start decaying in [b]{0}[/b].
              [br]After that it will decay in [b]{1}[/b] and then start the destruction timer ([b]{2}[/b]).
              [br]
              [br]In order to avoid the decay, simply visit this base at least once in [b]{0}[/b]
              [br](any player from the access list can trigger the reset of the decay timer).
              [br]
              [br]You can increase the decay delay by [b]upgrading[/b] any of the land claims to a higher tier.
              [br]The decay delay of the entire base is based on the [b]highest[/b] tier land claim building.";

    public const string DestructionTimeoutOnlyInPvP = "only in PvP";

    private readonly ILogicObject area;

    private readonly IStaticWorldObject landClaimWorldObject;

    private readonly LandClaimAreaPrivateState privateState;

    private readonly IProtoObjectLandClaim protoObjectLandClaim;

    private bool isSafeStorageTabSelected;

    public ViewModelWindowLandClaim(
        IStaticWorldObject landClaimWorldObject,
        ILogicObject area)
    {
      this.landClaimWorldObject = landClaimWorldObject;
      this.area = area;
      this.privateState = LandClaimArea.GetPrivateState(area);

      var protoLandClaim = (IProtoObjectLandClaim)landClaimWorldObject.ProtoStaticWorldObject;
      var canEditOwners = protoLandClaim
          .SharedCanEditOwners(landClaimWorldObject, ClientCurrentCharacterHelper.Character);

      if (!this.IsOwnedByFaction)
      {
        this.ViewModelOwnersEditor = new ViewModelWorldObjectOwnersEditor(
            this.privateState.DirectLandOwners,
            callbackServerSetOwnersList: ownersList => LandClaimSystem.ClientSetAreaOwners(
                                             area,
                                             ownersList),
            title: AccessListTitle + ":",
            emptyListMessage: AccessListEmpty,
            canEditOwners: canEditOwners,
            // exclude founder name
            ownersListFilter: name => name != this.FounderName,
            maxOwnersListLength: RateLandClaimOwnersMax.SharedValue,
            displayedOwnersNumberAdjustment: -1);
      }

      this.protoObjectLandClaim =
          (IProtoObjectLandClaim)this.landClaimWorldObject.ProtoStaticWorldObject;

      var upgrade = this.protoObjectLandClaim.ConfigUpgrade.Entries.FirstOrDefault();
      if (upgrade is not null)
      {
        this.ViewModelStructureUpgrade = new ViewModelStructureUpgrade(upgrade);
        this.ViewModelProtoLandClaimInfoUpgrade = new ViewModelProtoLandClaimInfo(
            (IProtoObjectLandClaim)upgrade.ProtoStructure);
      }

      var objectPublicState = landClaimWorldObject.GetPublicState<ObjectLandClaimPublicState>();
      objectPublicState.ClientSubscribe(
          _ => _.LandClaimAreaObject,
          _ => this.RefreshSafeStorageAndPowerGrid(),
          this);

      this.RefreshSafeStorageAndPowerGrid();


      //MOD
      objectPublicState.ClientSubscribe(
          _ => _.LandClaimAreaObject,
          _ => this.RefreshRobotSetings(),
          this);

      this.RefreshRobotSetings();

      this.ViewModelProtoLandClaimInfoCurrent = new ViewModelProtoLandClaimInfo(this.protoObjectLandClaim);

      RateResourcesGatherBasic.ClientValueChanged
          += this.SafeItemsSlotsCapacityChangedHandler;

      FactionConstants.ClientFactionMembersMaxChanged
          += this.FactionMembersMaxChangedHandler;

      this.RequestDecayInfoTextAsync();

      this.ViewModelShieldProtectionControl = new ViewModelShieldProtectionControl(
          LandClaimSystem.SharedGetLandClaimAreasGroup(area));
    }

    public bool CanOpenTransferToFactionOwnershipDialog
        => !this.IsOwnedByFaction;

    public BaseCommand CommandConfirmLandClaimDecayMessage
        => new ActionCommand(this.ExecuteCommandConfirmLandClaimDecayMessage);

    public BaseCommand CommandTransferLandClaimToFactionOwnership
        => new ActionCommand(this.ExecuteCommandTransferLandClaimToFactionOwnership);

    public BaseCommand CommandUpgrade
        => new ActionCommand(this.ExecuteCommandUpgrade);

    public string DecayInfoText { get; private set; } = CoreStrings.PleaseWait;

    public string FounderName => this.privateState.LandClaimFounder;

    public bool IsDecayEnabled => RateStructuresDecayEnabled.SharedValue;

    public bool IsLandClaimDecayInfoConfirmed
            => !this.IsDecayEnabled
               || LandClaimDecayInfoConfirmationHelper.IsConfirmedForCurrentServer;

    public bool IsMaxFactionSizeLargerThanSinglePlayer
        => FactionConstants.SharedMembersMaxPrivateFaction > 1;

    public bool IsOwnedByFaction
        => LandClaimSystem.SharedIsAreaOwnedByFaction(this.area);

    public bool IsPlayerHasUpgradeRight
        => this.FounderName == ClientCurrentCharacterHelper.Character.Name
           || (this.IsOwnedByFaction
               && FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.LandClaimManagement))
           || CreativeModeSystem.ClientIsInCreativeMode();

    public bool IsSafeStorageAvailable
        => RatePvPSafeStorageCapacity.SharedValue > 0;

    public bool IsSafeStorageCapacityExceeded
        => this.ViewModelSafeStorageItemsContainerExchange.Container.SlotsCount
           > RatePvPSafeStorageCapacity.SharedValue;

    public bool IsSafeStorageTabSelected
    {
      get => this.isSafeStorageTabSelected;
      set
      {
        if (this.isSafeStorageTabSelected == value)
        {
          return;
        }

        this.isSafeStorageTabSelected = value;
        this.NotifyThisPropertyChanged();
      }
    }

    public string OwnedByFactionClanTag
        => LandClaimSystem.SharedGetAreaOwnerFactionClanTag(this.area);

    public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }

    public ViewModelPowerGridState ViewModelPowerGridState { get; private set; }

    //MOD
    public ViewModelRobotState ViewModelRobotState { get; private set; }

    public ViewModelProtoLandClaimInfo ViewModelProtoLandClaimInfoCurrent { get; }

    public ViewModelProtoLandClaimInfo ViewModelProtoLandClaimInfoUpgrade { get; }

    public ViewModelItemsContainerExchange ViewModelSafeStorageItemsContainerExchange { get; set; }

    public ViewModelShieldProtectionControl ViewModelShieldProtectionControl { get; }

    public ViewModelStructureUpgrade ViewModelStructureUpgrade { get; }

    protected override void DisposeViewModel()
    {
      RateResourcesGatherBasic.ClientValueChanged
          -= this.SafeItemsSlotsCapacityChangedHandler;

      FactionConstants.ClientFactionMembersMaxChanged
          -= this.FactionMembersMaxChangedHandler;

      this.DisposeViewModelItemsContainerExchange();
      base.DisposeViewModel();
    }

    private void DisposeViewModelItemsContainerExchange()
    {
      var viewModel = this.ViewModelSafeStorageItemsContainerExchange;
      if (viewModel is null)
      {
        return;
      }

      this.ViewModelSafeStorageItemsContainerExchange = null;

      var container = viewModel.Container;
      container.SlotsCountChanged -= this.SafeStorageSlotsChangedHandler;
      container.ItemsReset -= this.SafeStorageSlotsChangedHandler;
      container.ItemAdded -= this.SafeStorageItemAddedHandler;
      container.ItemCountChanged -= this.SafeStorageItemCountChangedHandler;

      viewModel.Dispose();
    }

    private void ExecuteCommandConfirmLandClaimDecayMessage()
    {
      LandClaimDecayInfoConfirmationHelper.SetConfirmedForCurrentServer();
      this.NotifyPropertyChanged(nameof(this.IsLandClaimDecayInfoConfirmed));
    }

    private void ExecuteCommandTransferLandClaimToFactionOwnership()
    {
      var accessRight = FactionMemberAccessRights.LandClaimManagement;
      var hasAccessRight = FactionSystem.ClientHasAccessRight(accessRight);

      var stackPanel = new StackPanel() { Orientation = Orientation.Vertical };
      stackPanel.Children.Add(
          DialogWindow.CreateTextElement(CoreStrings.Faction_DialogTransferLandClaimToFaction,
                                         TextAlignment.Left));

      {
        // a warning about the potential access loss
        var textElement = DialogWindow.CreateTextElement(
            "[br]"
            + CoreStrings.Faction_DialogTransferLandClaimToFaction_PotentialAccessLoss,
            TextAlignment.Left);

        textElement.Foreground = Client.UI.GetApplicationResource<Brush>("BrushColor6");
        stackPanel.Children.Add(textElement);
      }

      if (!hasAccessRight)
      {
        var message = FactionSystem.ClientHasFaction
                          ? string.Format(
                              CoreStrings.Faction_Permission_Required_Format,
                              accessRight.GetDescription())
                          : CoreStrings.Faction_ErrorDontHaveFaction;

        var textElement = DialogWindow.CreateTextElement("[br]" + message,
                                                         TextAlignment.Left);

        textElement.Foreground = Client.UI.GetApplicationResource<Brush>("BrushColorRed6");
        textElement.FontWeight = FontWeights.Bold;
        stackPanel.Children.Add(textElement);
      }

      var dialog = DialogWindow.ShowDialog(
          CoreStrings.QuestionAreYouSure,
          stackPanel,
          okAction: () => LandClaimSystem.ClientTransferLandClaimToFactionOwnership(this.area),
          cancelAction: () => { },
          okText: CoreStrings.Button_Continue,
          focusOnCancelButton: true);

      if (!hasAccessRight)
      {
        dialog.ButtonOk.IsEnabled = false;
      }
    }

    private void ExecuteCommandUpgrade()
    {
      var upgradeStructure = this.ViewModelStructureUpgrade.ViewModelUpgradedStructure.ProtoStructure;

      // please note: it will perform all the checks and display the error dialog if required
      this.protoObjectLandClaim.ClientUpgrade(
          this.landClaimWorldObject,
          (IProtoObjectLandClaim)upgradeStructure);
    }

    private void FactionMembersMaxChangedHandler()
    {
      this.NotifyPropertyChanged(nameof(this.IsMaxFactionSizeLargerThanSinglePlayer));
    }

    /// <summary>
    /// MOD
    /// </summary>
    private void RefreshRobotSetings()
    {
      //var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(this.area);
      //var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);

      var oldViewModelRobotState = this.ViewModelRobotState;
      this.ViewModelRobotState = new ViewModelRobotState(this.area.GetPrivateState<LandClaimAreaPrivateState>(), this.landClaimWorldObject);
      oldViewModelRobotState?.Dispose();
    }

    private void RefreshSafeStorageAndPowerGrid()
    {
      var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(this.area);
      var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);

      // setup power grid
      var powerGrid = areasGroupPrivateState.PowerGrid;
      var oldViewModelPowerGridState = this.ViewModelPowerGridState;
      this.ViewModelPowerGridState = new ViewModelPowerGridState(Systems.PowerGridSystem.PowerGrid.GetPublicState(powerGrid));
      oldViewModelPowerGridState?.Dispose();

      // setup safe storage
      this.DisposeViewModelItemsContainerExchange();

      this.ViewModelSafeStorageItemsContainerExchange = new ViewModelItemsContainerExchange(
              areasGroupPrivateState.ItemsContainerSafeStorage,
              enableShortcuts: this.IsSafeStorageAvailable)
      {
        IsContainerTitleVisible = false
      };

      var container = this.ViewModelSafeStorageItemsContainerExchange.Container;
      container.SlotsCountChanged += this.SafeStorageSlotsChangedHandler;
      container.ItemsReset += this.SafeStorageSlotsChangedHandler;
      container.ItemAdded += this.SafeStorageItemAddedHandler;
      container.ItemCountChanged += this.SafeStorageItemCountChangedHandler;
    }

    private async void RequestDecayInfoTextAsync()
    {
      if (!this.IsDecayEnabled)
      {
        return;
      }

      var result = await LandClaimSystem.ClientGetDecayInfoText(this.landClaimWorldObject);
      if (this.IsDisposed)
      {
        return;
      }

      var decayDelayDurationText = ClientTimeFormatHelper.FormatTimeDuration(
          TimeSpan.FromSeconds(result.DecayDelayDuration),
          trimRemainder: true);

      var decayDurationText = ClientTimeFormatHelper.FormatTimeDuration(
          TimeSpan.FromSeconds(result.DecayDuration),
          trimRemainder: true);

      var destructionTimeout =
          this.ViewModelProtoLandClaimInfoCurrent.CurrentStructureLandClaimDestructionTimeout;

      var text = string.Format(DecayInfoFormat,
                               decayDelayDurationText,
                               decayDurationText,
                               destructionTimeout ?? DestructionTimeoutOnlyInPvP);

      if (result.IsFounderDemoPlayer)
      {
        text += "[br][br]" + DecayInfoDemoVersion;
      }

      this.DecayInfoText = text;
    }

    private void SafeItemsSlotsCapacityChangedHandler()
    {
      this.RefreshSafeStorageAndPowerGrid();
    }

    private void SafeStorageItemAddedHandler(IItem item)
    {
      this.IsSafeStorageTabSelected = true;
    }

    private void SafeStorageItemCountChangedHandler(IItem item, ushort previousCount, ushort currentCount)
    {
      if (currentCount > previousCount)
      {
        this.IsSafeStorageTabSelected = true;
      }
    }

    private void SafeStorageSlotsChangedHandler()
    {
      this.NotifyPropertyChanged(nameof(this.IsSafeStorageCapacityExceeded));
    }
  }
}