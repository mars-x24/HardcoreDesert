namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
  using System;
  using System.Collections.Generic;
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.CoreMod.UI;
  using AtomicTorch.CBND.CoreMod.Vehicles;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.GameEngine.Common.Extensions;

  public abstract class ProtoItemVehicleWeaponMeleeEnergy
    <TPrivateState,
     TPublicState,
     TClientState>
    : ProtoItemWeaponMeleeEnergy
      <TPrivateState,
          TPublicState,
          TClientState>,
      IProtoItemVehicleWeapon
    where TPrivateState : WeaponPrivateState, new()
    where TPublicState : BasePublicState, new()
    where TClientState : BaseClientState, new()
  {
    public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

    public override string WeaponAttachmentName => "TurretLeft";

    public override string GetCharacterAnimationNameFire(ICharacter character)
    {
      return "";
    }

    public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
    {
      if (IsClient
          && !isAlreadySelected
          && isByPlayer)
      {
        NotificationSystem.ClientShowNotification(
            this.Name,
            CoreStrings.Vehicle_Mech_NotificationWeaponNeedsInstallationOnMech,
            NotificationColor.Bad,
            item.ProtoItem.Icon);
      }

      return false;
    }

    public abstract VehicleWeaponHardpoint WeaponHardpoint { get; }

    protected override string GenerateIconPath()
    {
      return "Items/Weapons/Melee/" + this.GetType().Name;
    }

    protected override void PrepareHints(List<string> hints)
    {
      base.PrepareHints(hints);

      hints.Add(string.Format(ItemHints.VehicleWeapon_Format,
                              this.WeaponHardpoint.GetDescription()));
    }

    public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
    {
      var requiredEnergyAmount = this.EnergyUsePerShot;
      var vehicle = character.SharedGetCurrentVehicle();
      if (VehicleEnergySystem.SharedHasEnergyCharge(vehicle, Convert.ToUInt16(requiredEnergyAmount)))
      {
        return true;
      }

      if (IsClient && weaponState.SharedGetInputIsFiring())
      {
        VehicleEnergyConsumptionSystem.ClientShowNotificationNotEnoughEnergy(
            (IProtoVehicle)vehicle.ProtoGameObject);
        // stop using weapon item!
        weaponState.ProtoWeapon.ClientItemUseFinish(weaponState.ItemWeapon);
      }

      return false;
    }

    // Please note: the check SharedCanFire() has been already passed
    public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
    {
      if (IsClient)
      {
        // on client we cannot consume energy
        return true;
      }

      var requiredEnergyAmount = this.EnergyUsePerShot;
      var vehicle = character.SharedGetCurrentVehicle();
      return VehicleEnergySystem.ServerDeductEnergyCharge(vehicle, Convert.ToUInt16(requiredEnergyAmount));
    }
  }

  public abstract class ProtoItemVehicleWeaponMeleeEnergy
    : ProtoItemVehicleWeaponMeleeEnergy
        <WeaponPrivateState, EmptyPublicState, EmptyClientState>
  {
  }
}