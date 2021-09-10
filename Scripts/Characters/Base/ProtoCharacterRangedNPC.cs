using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using System;
using System.Threading.Tasks;

namespace AtomicTorch.CBND.CoreMod.Characters
{
  public abstract class ProtoCharacterRangedNPC : ProtoCharacterNPC
  {
    public virtual double RetreatDistance => 7;

    public virtual double EnemyToCloseDistance => 7;

    public virtual double EnemyToFarDistance => 14;

    public virtual bool RetreatWhenReloading => true;

    public virtual double RetreatingHealthPercentage => 50.0;

    protected override void ServerUpdateMob(ServerUpdateData data)
    {
      var character = data.GameObject;
      var currentStats = data.PublicState.CurrentStats;
      var closestTarget = ServerNpcAiHelper.GetClosestTargetPlayer(character);

      var privateState = character.GetPrivateState<CharacterMobNPCPrivateState>();
      var weaponState = privateState.WeaponState;
      var weapon = weaponState.ProtoWeapon;

      if (privateState.IsReloading)
      {
        privateState.ReloadTimer -= data.DeltaTime;
        if (privateState.ReloadTimer <= 0.0)
        {
          privateState.ReloadTimer = 0.0;
          privateState.IsReloading = false;
          PlayReloadSoundFinished(weaponState, character);
        }
      }
      else if (closestTarget is not null && !closestTarget.IsNpc)
      {
        if (weaponState.SharedGetInputIsFiring())
        {
          ushort ammoConsumptionPerShot = weapon.AmmoConsumptionPerShot == (byte)0 ? (byte)1 : weapon.AmmoConsumptionPerShot;
          double ammosFired = ammoConsumptionPerShot / weapon.FireInterval * data.DeltaTime;

          privateState.AmmosFired += ammosFired;

          if (privateState.AmmosFired > weapon.AmmoCapacity)
          {
            privateState.AmmosFired = 0;
            privateState.IsReloading = true;
            privateState.ReloadTimer = weapon.AmmoReloadDuration;
            PlayReloadSound(weaponState, character);
          }
        }
      }


      ServerNpcAiHelper.ProcessAggressiveAi(
        character,
        targetCharacter: closestTarget,
        isRetreating: currentStats.HealthCurrent < currentStats.HealthMax * (RetreatingHealthPercentage / 100) || (RetreatWhenReloading && privateState.IsReloading),
        isRetreatingForHeavyVehicles: false,
        distanceRetreat: RetreatDistance,
        distanceEnemyTooClose: EnemyToCloseDistance,
        distanceEnemyTooFar: EnemyToFarDistance,
        movementDirection: out var movementDirection,
        rotationAngleRad: out var rotationAngleRad,
        attackFarOnlyIfAggro: false,
        isReloading: privateState.IsReloading);

      this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
    }

    public static void PlayReloadSound(WeaponState weaponState, ICharacter character)
    {
      if (IsClient)
      {
        weaponState.ProtoWeapon.SoundPresetWeapon
                        .PlaySound(WeaponSound.Reload,
                                    character,
                                    SoundConstants.VolumeWeapon);
      }
      else
      {
        ServerNotifyAboutReloading(character, weaponState, isFinished: false);
      }

    }

    public static void PlayReloadSoundFinished(WeaponState weaponState, ICharacter character)
    {
      if (IsClient)
      {
        weaponState.ProtoWeapon.SoundPresetWeapon
                        .PlaySound(WeaponSound.ReloadFinished,
                                    character,
                                    SoundConstants.VolumeWeapon);
      }
      else
      {
        ServerNotifyAboutReloading(character, weaponState, isFinished: true);
      }

    }

    // send notification about reloading to players in scope (so they can play a sound)
    private static void ServerNotifyAboutReloading(ICharacter character, WeaponState weaponState, bool isFinished)
    {
      using var scopedBy = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(character, scopedBy);
      scopedBy.Remove(character);
      if (scopedBy.Count == 0)
      {
        return;
      }

      if (isFinished)
      {
        WeaponAmmoSystem.Instance.CallClient(scopedBy.AsList(),
                            _ => _.ClientRemote_OnOtherCharacterReloaded(character, weaponState.ProtoWeapon));
      }
      else
      {
        WeaponAmmoSystem.Instance.CallClient(scopedBy.AsList(),
                            _ => _.ClientRemote_OnOtherCharacterReloading(character, weaponState.ProtoWeapon));
      }
    }
  }
}
