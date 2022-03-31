﻿using AtomicTorch.CBND.CoreMod.Systems.Weapons;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Characters
{
  public abstract class ProtoCharacterMobEnraged
      <TPrivateState,
       TPublicState,
       TClientState>
      : ProtoCharacterMob
        <TPrivateState,
            TPublicState,
            TClientState>,
        IProtoCharacterMob,
        IProtoDamageToNPC
      where TPrivateState : CharacterMobEnragedPrivateState, new()
      where TPublicState : CharacterMobPublicState, new()
      where TClientState : BaseCharacterClientState, new()
  {
    protected override bool IsAutoDespawn => false;

    public virtual float CharacterWorldWeaponOffsetMeleeX => 0.0f;//0.5f;

    public override bool AiIsRunAwayFromHeavyVehicles => false;

    public override bool CanBeSpawnedByMobs => false;

    protected override double LifeTime => 60 * 60;

    //the npc is attacking himself with the test line
    public override Vector2D SharedGetWeaponFireWorldPosition(ICharacter character, bool isMeleeWeapon)
    {
      var characterRotationAngleRad = ((IProtoCharacterCore)character.ProtoCharacter)
        .SharedGetRotationAngleRad(character);

      return character.Position + (0, CharacterWorldWeaponOffsetMelee)
             + new Vector2D(CharacterWorldWeaponOffsetMeleeX, 0)
                 .RotateRad(characterRotationAngleRad);

    }

    public override bool SharedOnDamage(WeaponFinalCache weaponCache, IWorldObject targetObject, double damagePreMultiplier, double damagePostMultiplier, out double obstacleBlockDamageCoef, out double damageApplied)
    {
      damageApplied = 0;
      obstacleBlockDamageCoef = 0;

      if (weaponCache.Character?.ProtoGameObject is ProtoCharacterMobEnraged && targetObject.ProtoGameObject is ProtoCharacterMobEnraged)
        return false;

      return base.SharedOnDamage(weaponCache, targetObject, damagePreMultiplier, damagePostMultiplier, out obstacleBlockDamageCoef, out damageApplied);
    }
  }

  public abstract class ProtoCharacterMobEnraged
      : ProtoCharacterMobEnraged
          <CharacterMobEnragedPrivateState,
              CharacterMobPublicState,
              CharacterMobClientState>
  {
  }
}