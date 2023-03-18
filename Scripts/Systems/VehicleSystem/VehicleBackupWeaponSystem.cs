using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Scripting.Network;

namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
  public class VehicleBackupWeaponSystem : ProtoSystem<VehicleBackupWeaponSystem>
  {
    public static void ClientTrySwitchingWeapon()
    {
      if (!IsClient)
        return;

      if (!SharedTrySwitchWeapon(Client.Characters.CurrentPlayerCharacter))
        return;

      Instance.CallServer(_ => _.ServerRemote_TrySwitchingWeapon());
    }

    private void ServerRemote_TrySwitchingWeapon()
    {
      var character = ServerRemoteContext.Character;
      SharedTrySwitchWeapon(character);
    }

    private static bool SharedTrySwitchWeapon(ICharacter character)
    {
      var publicState = character.GetPublicState<PlayerCharacterPublicState>();
      var vehicle = publicState.CurrentVehicle;
      bool hasVehicle = vehicle is not null;

      if (!hasVehicle
          || !vehicle.IsInitialized
          || !vehicle.ClientHasPrivateState)
        return false;

      var vehicleProto = (IProtoVehicle)vehicle.ProtoGameObject;
      if (vehicleProto.IsPlayersHotbarAndEquipmentItemsAllowed)
        return false;

      var containerBackup = vehicleProto.SharedGetHotbarItemsContainerBackup(vehicle);
      if (containerBackup is null)
        return false;

      if (IsServer)
      {
        if (vehicleProto.ServerUseContainerBackup(vehicle))
          PlayerCharacter.SharedSelectHotbarSlotId(character, 0, false);
        //SharedForceRefreshCurrentItem(character);
      }

      return true;
    }
  }
}