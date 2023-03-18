using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
using AtomicTorch.CBND.CoreMod.Helpers.Client;
using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.Rates;
using AtomicTorch.CBND.CoreMod.StaticObjects;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Vehicles
{
  public abstract class ProtoVehicleMech
        <TVehiclePrivateState,
         TVehiclePublicState,
         TVehicleClientState>
        : ProtoVehicle
            <TVehiclePrivateState,
                TVehiclePublicState,
                TVehicleClientState>
        where TVehiclePrivateState : VehicleMechPrivateState, new()
        where TVehiclePublicState : VehicleMechPublicState, new()
        where TVehicleClientState : VehicleClientState, new()
  {
    public abstract BaseItemsContainerMechEquipment EquipmentItemsContainerType { get; }

    public override bool IsAllowCreatureDamageWhenNoPilot => true;

    public override bool IsArmorBarDisplayedWhenPiloted => true;

    public override bool IsHeavyVehicle => true;

    public override bool IsPlayersHotbarAndEquipmentItemsAllowed => false;

    public override ITextureResource MapIcon => new TextureResource("Icons/MapExtras/VehicleMech");

    public override double MaxDistanceToInteract => 1;

    public override float ObjectSoundRadius => 2;

    public override SoundResource SoundResourceLightsToggle { get; }
        = new("Objects/Vehicles/Mech/ToggleLight");

    public override SoundResource SoundResourceVehicleDismount { get; }
        = new("Objects/Vehicles/Mech/Dismount");

    public override SoundResource SoundResourceVehicleMount { get; }
        = new("Objects/Vehicles/Mech/Mount");

    public override double StatMoveSpeedRunMultiplier => 1.0; // no run mode

    public override void ClientOnVehicleDismounted(IDynamicWorldObject vehicle)
    {
      base.ClientOnVehicleDismounted(vehicle);

      var skeletonRenderer = GetClientState(vehicle).SkeletonRenderer;
      if (skeletonRenderer is null)
      {
        return;
      }

      // switch animation to idle and then to offline to ensure both are played
      skeletonRenderer.SetAnimationFrame(0, "Idle", timePositionFraction: 0);
      skeletonRenderer.SetAnimation(0, "Offline", isLooped: false);
    }

    public override BaseUserControlWithWindow ClientOpenUI(IWorldObject worldObject)
    {
      var privateState = GetPrivateState((IDynamicWorldObject)worldObject);
      return WindowObjectVehicle.Open(
          (IDynamicWorldObject)worldObject,
          vehicleExtraControl: new ControlMechEquipment(),
          vehicleExtraControlViewModel: new ViewModelControlMechEquipment(privateState));
    }

    public override void ServerOnDestroy(IDynamicWorldObject gameObject)
    {
      var publicState = GetPublicState(gameObject);
      var privateState = GetPrivateState(gameObject);
      var pilotCharacter = publicState.PilotCharacter
                           ?? privateState.ServerLastPilotCharacter;

      base.ServerOnDestroy(gameObject);

      // try drop extra containers on the ground
      var objectGroundContainer = ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
          gameObject.Tile,
          privateState.EquipmentItemsContainer,
          DestroyedCargoDroppedItemsDestructionTimeout.TotalSeconds);
      ServerTryClaimGroundContainerWithDroppedGoods(objectGroundContainer, pilotCharacter, privateState);

      var objectGroundContainer2 = ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
        gameObject.Tile,
        privateState.EquipmentItemsContainerBackup,
        DestroyedCargoDroppedItemsDestructionTimeout.TotalSeconds);
      ServerTryClaimGroundContainerWithDroppedGoods(objectGroundContainer2, pilotCharacter, privateState);
      // assume that the fuel items were destroyed during the explosion
    }

    public override IItemsContainer SharedGetHotbarItemsContainer(IDynamicWorldObject vehicle)
    {
      return GetPrivateState(vehicle).EquipmentItemsContainer;
    }
    public override IItemsContainer SharedGetHotbarItemsContainerBackup(IDynamicWorldObject vehicle)
    {
      return GetPrivateState(vehicle).EquipmentItemsContainerBackup;
    }

    public override bool ServerUseContainerBackup(IDynamicWorldObject vehicle)
    {
      if (Api.IsClient)
        return false;

      var privateState = GetPrivateState(vehicle);

      var containerBackup = privateState.EquipmentItemsContainerBackup;
      if (containerBackup is null)
        return false;

      var weapon = containerBackup.GetItemAtSlot(0);
      if (weapon is null)
        return false;

      var container = privateState.EquipmentItemsContainer;
      var containerTemp = privateState.EquipmentItemsContainerTemp;

      //privateState.EquipmentItemsContainer = privateState.EquipmentItemsContainerTemp;
      //privateState.EquipmentItemsContainerBackup = container;
      //privateState.EquipmentItemsContainer = backupContainer;

      var publicState = GetPublicState(vehicle);
      publicState.ProtoItemLeftTurretSlot = null;

      //move temp to cargo
      for (byte i = 0; i < containerTemp.SlotsCount; i++)
      {
        if (containerTemp.IsSlotOccupied(i))
          Api.Server.Items.MoveOrSwapItem(containerTemp.GetItemAtSlot(i), privateState.CargoItemsContainer, out _);
      }

      if (RateVehicleBackupWeaponEnabled.SharedValue)
      {
        //move main to temp
        for (byte i = 0; i < container.SlotsCount; i++)
        {
          if (container.IsSlotOccupied(i))
            Api.Server.Items.MoveOrSwapItem(container.GetItemAtSlot(i), containerTemp, out _, i);
        }

        //move backup to main
        for (byte i = 0; i < containerBackup.SlotsCount; i++)
        {
          if (containerBackup.IsSlotOccupied(i))
            Api.Server.Items.MoveOrSwapItem(containerBackup.GetItemAtSlot(i), container, out _, i);
        }

        //move temp to backup
        for (byte i = 0; i < containerTemp.SlotsCount; i++)
        {
          if (containerTemp.IsSlotOccupied(i))
            Api.Server.Items.MoveOrSwapItem(containerTemp.GetItemAtSlot(i), containerBackup, out _);
        }

        return true;
      }
      else
      {
        //move backup to cargo
        for (byte i = 0; i < containerBackup.SlotsCount; i++)
        {
          if (containerBackup.IsSlotOccupied(i))
            Api.Server.Items.MoveOrSwapItem(containerBackup.GetItemAtSlot(i), privateState.CargoItemsContainer, out _);
        }

        return false;
      }
    }

    protected override void ClientInitializeVehicle(ClientInitializeData data)
    {
      base.ClientInitializeVehicle(data);

      var vehicle = data.GameObject;
      var publicState = data.PublicState;
      var clientState = data.ClientState;

      // re-create rendering when turret items changed
      publicState.ClientSubscribe(c => c.ProtoItemLeftTurretSlot,
                                  _ => vehicle.ClientInitialize(),
                                  clientState);

      publicState.ClientSubscribe(c => c.ProtoItemRightTurretSlot,
                                  _ => vehicle.ClientInitialize(),
                                  clientState);
    }

    protected override void ClientSetupCurrentPlayerUI(IDynamicWorldObject vehicle)
    {
      base.ClientSetupCurrentPlayerUI(vehicle);

      GetClientState(vehicle).UIElementsHolder = new ClientVehicleMechCurrentPlayerUIController(vehicle);

      ClientCurrentCharacterVehicleContainersHelper.Init(new[]
      {
                (IClientItemsContainer)GetPrivateState(vehicle).EquipmentItemsContainer
            });
    }

    protected override void ClientSetupRendering(ClientInitializeData data)
    {
      base.ClientSetupRendering(data);

      var publicState = data.PublicState;

      var skeletonRenderer = data.ClientState.SkeletonRenderer;
      if (skeletonRenderer is null)
      {
        return;
      }

      this.SharedGetSkeletonProto(data.GameObject, out var protoSkeleton, out _);
      SetupItem(publicState.ProtoItemLeftTurretSlot);
      SetupItem(publicState.ProtoItemRightTurretSlot);

      skeletonRenderer.SetAnimationFrame(0, "Offline", timePositionFraction: 1);

      void SetupItem(IProtoItem protoItem)
      {
        if (protoItem is IProtoItemWithCharacterAppearance protoItemWithCharacterAppearance)
        {
          protoItemWithCharacterAppearance.ClientSetupSkeleton(
              item: null,
              character: null,
              protoCharacterSkeleton: (ProtoCharacterSkeleton)protoSkeleton,
              skeletonRenderer: skeletonRenderer,
              skeletonComponents: new List<IClientComponent>());
        }
      }
    }

    protected override void ClientTryDestroyCurrentPlayerUI(IDynamicWorldObject gameObject)
    {
      base.ClientTryDestroyCurrentPlayerUI(gameObject);

      if (!gameObject.ClientHasPrivateState)
      {
        return;
      }

      var privateState = GetPrivateState(gameObject);
      ClientCurrentCharacterVehicleContainersHelper.Reset(new[]
      {
                (IClientItemsContainer)privateState.EquipmentItemsContainer
            });
    }

    protected override void ServerInitializeVehicle(ServerInitializeData data)
    {
      base.ServerInitializeVehicle(data);

      var worldObject = data.GameObject;
      var privateState = data.PrivateState;

      // setup equipment items container
      var equipmentItemsContainer = privateState.EquipmentItemsContainer;
      var equipmentItemsSlotsCount = this.EquipmentItemsContainerType.TotalSlotsCount;
      if (equipmentItemsContainer is not null)
      {
        // container already created - update it
        Server.Items.SetContainerType(equipmentItemsContainer, this.EquipmentItemsContainerType);
        Server.Items.SetSlotsCount(equipmentItemsContainer,
                                   slotsCount: equipmentItemsSlotsCount);
      }
      else
      {
        equipmentItemsContainer = Server.Items.CreateContainer(
            owner: worldObject,
            itemsContainerType: this.EquipmentItemsContainerType,
            slotsCount: equipmentItemsSlotsCount);

        privateState.EquipmentItemsContainer = equipmentItemsContainer;
      }

      // setup equipment items container backup
      var equipmentItemsContainerBackup = privateState.EquipmentItemsContainerBackup;
      if (equipmentItemsContainerBackup is not null)
      {
        // container already created - update it
        Server.Items.SetContainerType(equipmentItemsContainerBackup, this.EquipmentItemsContainerType);
        Server.Items.SetSlotsCount(equipmentItemsContainerBackup,
                                   slotsCount: equipmentItemsSlotsCount);
      }
      else
      {
        equipmentItemsContainerBackup = Server.Items.CreateContainer(
            owner: worldObject,
            itemsContainerType: this.EquipmentItemsContainerType,
            slotsCount: equipmentItemsSlotsCount);

        privateState.EquipmentItemsContainerBackup = equipmentItemsContainerBackup;
      }

      // setup equipment items container temp
      var equipmentItemsContainerTemp = privateState.EquipmentItemsContainerTemp;
      if (equipmentItemsContainerTemp is not null)
      {
        // container already created - update it
        Server.Items.SetContainerType(equipmentItemsContainerTemp, this.EquipmentItemsContainerType);
        Server.Items.SetSlotsCount(equipmentItemsContainerTemp,
                                   slotsCount: equipmentItemsSlotsCount);
      }
      else
      {
        equipmentItemsContainerTemp = Server.Items.CreateContainer(
            owner: worldObject,
            itemsContainerType: this.EquipmentItemsContainerType,
            slotsCount: equipmentItemsSlotsCount);

        privateState.EquipmentItemsContainerTemp = equipmentItemsContainerTemp;
      }
    }

    protected override void ServerUpdateVehicle(ServerUpdateData data)
    {
      var privateState = data.PrivateState;
      var publicState = data.PublicState;

      publicState.ProtoItemLeftTurretSlot = privateState.EquipmentItemsContainer.GetItemAtSlot(0)?.ProtoItem;
      // not used yet
      //publicState.ProtoItemSlotRightTurret = privateState.EquipmentItemsContainer.GetItemAtSlot(1)?.ProtoItem;
    }

    protected override double SharedCalculateDamageByWeapon(
        WeaponFinalCache weaponCache,
        double damagePreMultiplier,
        IDynamicWorldObject targetObject,
        out double obstacleBlockDamageCoef)
    {
      var damage = base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
      if (weaponCache.Character?.ProtoGameObject is IProtoCharacterMob protoCharacterMob
          && protoCharacterMob.IsBoss)
      {
        // for balancing reasons we're increasing damage by boss
        damage *= 1.75;
      }

      return damage;
    }
  }

  public abstract class ProtoVehicleMech
      : ProtoVehicleMech
          <VehicleMechPrivateState,
              VehicleMechPublicState,
              VehicleClientState>
  {
  }
}