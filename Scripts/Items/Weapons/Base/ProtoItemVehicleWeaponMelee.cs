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

  public abstract class ProtoItemVehicleWeaponMelee
    <TPrivateState,
     TPublicState,
     TClientState>
    : ProtoItemWeaponMelee
      <TPrivateState,
          TPublicState,
          TClientState>,
      IProtoItemVehicleWeapon
    where TPrivateState : WeaponPrivateState, new()
    where TPublicState : BasePublicState, new()
    where TClientState : BaseClientState, new()
  {
    public override string WeaponAttachmentName => "TurretLeft";

    public abstract VehicleWeaponHardpoint WeaponHardpoint { get; }

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
  }

  public abstract class ProtoItemVehicleWeaponMelee
    : ProtoItemVehicleWeaponMelee
        <WeaponPrivateState, EmptyPublicState, EmptyClientState>
  {
  }
}