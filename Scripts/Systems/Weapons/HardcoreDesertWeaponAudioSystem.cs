using AtomicTorch.CBND.CoreMod.Items.Weapons;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Scripting.Network;

namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    /// <summary>
    /// Network audio notifications used by the HardcoreDesert NPC overrides.
    /// The Core methods have private visibility, so the mod owns these calls.
    /// </summary>
    public class HardcoreDesertWeaponAudioSystem : ProtoSystem<HardcoreDesertWeaponAudioSystem>
    {
        [RemoteCallSettings(DeliveryMode.Unreliable, timeInterval: 1 / 30.0, keyArgIndex: 0)]
        public void ClientRemote_OnNpcReloaded(ICharacter character, IProtoItemWeapon protoWeapon)
        {
            protoWeapon.SoundPresetWeapon.PlaySound(
                WeaponSound.ReloadFinished,
                character,
                SoundConstants.VolumeWeapon);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, timeInterval: 1 / 30.0, keyArgIndex: 0)]
        public void ClientRemote_OnNpcReloading(ICharacter character, IProtoItemWeapon protoWeapon)
        {
            protoWeapon.SoundPresetWeapon.PlaySound(
                WeaponSound.Reload,
                character,
                SoundConstants.VolumeWeapon);
        }
    }
}
