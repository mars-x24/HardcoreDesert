﻿using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
using AtomicTorch.CBND.CoreMod.Stats;
using AtomicTorch.CBND.CoreMod.Systems.Faction;
using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
using AtomicTorch.CBND.CoreMod.Systems.Party;
using AtomicTorch.CBND.CoreMod.Systems.PvE;
using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Data;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Weapons;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.GameEngine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
  public static class WeaponDamageSystem
  {
    public static readonly IReadOnlyDictionary<DamageType, StatName> DefenseStatNameBinding
        = Enum.GetValues(typeof(DamageType))
              .Cast<DamageType>()
              .ToDictionary(_ => _, SharedGetDefenseStatName);

    public static double ServerCalculateTotalDamage(
        WeaponFinalCache weaponCache,
        IWorldObject targetObject,
        FinalStatsCache targetFinalStatsCache,
        double damagePreMultiplier,
        bool clampDefenseTo1)
    {
      bool restrictedPvP = ServerIsRestrictedPvPDamage(weaponCache,
                                        targetObject,
                                        out var isPvPcase,
                                        out var isFriendlyFireCase);

      if (weaponCache.Character?.ProtoGameObject is not IProtoDamageToNPC) //MOD
      {
        if (targetObject is IStaticWorldObject staticWorldObject
            && (!RaidingProtectionSystem.SharedCanRaid(staticWorldObject,
                                                       showClientNotification: false)
                || !LandClaimShieldProtectionSystem.SharedCanRaid(staticWorldObject,
                                                                  showClientNotification: false)
                || !PveSystem.SharedIsAllowStaticObjectDamage(weaponCache.Character,
                                                              staticWorldObject,
                                                              showClientNotification: false)
                || !NewbieProtectionSystem.SharedIsAllowStructureDamage(weaponCache.Character,
                                                                        staticWorldObject,
                                                                        showClientNotification: false)))
        {
          return 0;
        }

        if (targetObject.ProtoGameObject is IProtoVehicle
            && !PveSystem.SharedIsAllowVehicleDamage(weaponCache,
                                                     (IDynamicWorldObject)targetObject,
                                                     showClientNotification: false))
        {
          return 0;
        }

        if (weaponCache.ProtoExplosive is not null
            && targetObject is IStaticWorldObject targetStaticWorldObject)
        {
          // special case - apply the explosive damage to static object
          return ServerCalculateTotalDamageByExplosive(weaponCache.Character,
                                                       weaponCache.ProtoExplosive,
                                                       targetStaticWorldObject,
                                                       damagePreMultiplier);
        }

        if (restrictedPvP)
        {
          return 0;
        }
      }


      if (weaponCache.Character?.ProtoGameObject is ProtoCharacterMobEnraged) //MOD
      {
        if (targetObject is IStaticWorldObject staticWorldObject2
            && !LandClaimShieldProtectionSystem.SharedCanRaid(staticWorldObject2,
                                                                showClientNotification: false))
        {
          return 0;
        }
      }

      var damageValue = damagePreMultiplier * weaponCache.DamageValue;
      var invertedArmorPiercingCoef = weaponCache.InvertedArmorPiercingCoef;

      var totalDamage = 0d;

      // calculate total damage by summing all the damage components
      foreach (var damageDistribution in weaponCache.DamageDistributions)
      {
        var defenseStatName = SharedGetDefenseStatName(damageDistribution.DamageType);
        var defenseFraction = targetFinalStatsCache[defenseStatName];
        defenseFraction = MathHelper.Clamp(defenseFraction, 0, clampDefenseTo1 ? 1 : double.MaxValue);

        totalDamage += ServerCalculateDamageComponent(
            damageValue,
            invertedArmorPiercingCoef,
            damageDistribution,
            defenseFraction);
      }

      // multiply on final multiplier (usually used for expanding projectiles)
      totalDamage *= weaponCache.FinalDamageMultiplier;

      if (weaponCache.ProtoExplosive is IProtoObjectExplosive)
      {
        // damage to character from a static object such as a bomb
        // - use the damage as is - damage rate multipliers are already applied to it
        return totalDamage;
      }

      var damagingCharacter = weaponCache.Character;
      if (isPvPcase)
      {
        // apply PvP damage multiplier
        totalDamage *= WeaponConstants.DamagePvpMultiplier;
      }
      else if (damagingCharacter is not null
               && !damagingCharacter.IsNpc
               && targetObject.ProtoGameObject
                   is IProtoCharacterMob protoCharacterMob
               && !protoCharacterMob.IsBoss)
      {
        // apply PvE damage multiplier
        totalDamage *= WeaponConstants.DamagePveMultiplier;
      }
      else if (damagingCharacter is not null
               && damagingCharacter.IsNpc)
      {
        // apply creature damage multiplier
        totalDamage *= WeaponConstants.DamageByCreaturesMultiplier;

        if (damagingCharacter.ProtoGameObject is IProtoCharacterMob { IsBoss: true })
        {
          // boss could always apply damage (even to offline player characters)
        }
        else if (targetObject is ICharacter { ServerIsOnline: false, IsNpc: false })
        {
          // don't deal creature damage to offline player characters
          totalDamage = 0;
        }
      }

      if (isFriendlyFireCase)
      {
        totalDamage *= WeaponConstants.DamageFriendlyFireMultiplier;
      }

      return totalDamage;
    }

    public static bool ServerIsFriendlyFire(ICharacter damagingCharacter, ICharacter targetCharacter)
    {
      if (damagingCharacter is null
          || damagingCharacter.IsNpc
          || targetCharacter is null
          || targetCharacter.IsNpc)
      {
        return false;
      }

      var targetParty = PartySystem.ServerGetParty(targetCharacter);
      if (targetParty is not null)
      {
        var damagingParty = PartySystem.ServerGetParty(damagingCharacter);
        if (targetParty == damagingParty)
        {
          // same party
          return true;
        }
      }

      var targetFaction = FactionSystem.ServerGetFaction(targetCharacter);
      if (targetFaction is null)
      {
        // target has no faction
        return false;
      }

      var damagingFaction = FactionSystem.ServerGetFaction(damagingCharacter);
      if (damagingFaction is null)
      {
        // damaging character has no faction
        return false;
      }

      if (damagingFaction == targetFaction)
      {
        // same faction
        return true;
      }

      var status = FactionSystem.ServerGetFactionDiplomacyStatus(targetFaction, damagingFaction);
      return status == FactionDiplomacyStatus.Ally; // friendly fire if ally
    }

    public static bool SharedCanHitCharacter(WeaponFinalCache weaponCache, ICharacter targetCharacter)
    {
      var targetPublicState = targetCharacter.GetPublicState<ICharacterPublicState>();
      var targetCurrentStats = targetPublicState.CurrentStats;

      if (targetCurrentStats.HealthCurrent <= 0)
      {
        // target character is dead, cannot apply damage to it
        return false;
      }

      var damagingCharacter = weaponCache.Character;
      if (!targetCharacter.IsNpc
          && damagingCharacter is not null
          && NewbieProtectionSystem.SharedIsNewbie(damagingCharacter))
      {
        // no PvP damage by newbie
        if (Api.IsClient)
        {
          // display message to newbie
          NewbieProtectionSystem.ClientShowNewbieCannotDamageOtherPlayersOrLootBags(isLootBag: false);
        }

        return false;
      }

      if (Api.IsClient)
      {
        // we don't simulate the damage on the client side
        if (damagingCharacter is not null)
        {
          // potentially a PvP case
          PveSystem.ClientShowDuelModeRequiredNotificationIfNecessary(
              damagingCharacter,
              targetCharacter);
        }

        return true;
      }

      if (damagingCharacter is not null
          && !weaponCache.AllowNpcToNpcDamage
          && damagingCharacter.IsNpc
          && (targetCharacter.IsNpc && targetCharacter.ProtoGameObject is not ProtoCharacterMobEnraged))
      {
        // no NPC-to-NPC damage
        return false;
      }

      return true;
    }

    public static StatName SharedGetDefenseStatName(DamageType damageType)
    {
      return damageType switch
      {
        DamageType.Impact => StatName.DefenseImpact,
        DamageType.Kinetic => StatName.DefenseKinetic,
        DamageType.Explosion => StatName.DefenseExplosion,
        DamageType.Heat => StatName.DefenseHeat,
        DamageType.Cold => StatName.DefenseCold,
        DamageType.Chemical => StatName.DefenseChemical,
        DamageType.Radiation => StatName.DefenseRadiation,
        DamageType.Psi => StatName.DefensePsi,
        _ => throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null)
      };
    }

    public static void SharedTryDamageCharacter(
        ICharacter targetCharacter,
        WeaponFinalCache weaponCache,
        double damagePreMultiplier,
        double damagePostMultiplier,
        out bool isHit,
        out double damageApplied)
    {
      isHit = SharedCanHitCharacter(weaponCache, targetCharacter);
      if (!isHit)
      {
        damageApplied = 0;
        return;
      }

      // hit registered
      if (Api.IsClient)
      {
        // damage is not calculated on the client side
        damageApplied = 0;
        return;
      }

      // calculate and apply damage on server
      var attackerCharacter = weaponCache.Character;
      var targetFinalStatsCache = targetCharacter.GetPrivateState<BaseCharacterPrivateState>()
                                                 .FinalStatsCache;

      var totalDamage = ServerCalculateTotalDamage(
          weaponCache,
          targetCharacter,
          targetFinalStatsCache,
          damagePreMultiplier,
          clampDefenseTo1: true);

      totalDamage *= damagePostMultiplier;

      if (totalDamage <= 0)
      {
        damageApplied = 0;
        return;
      }

      var targetPublicState = targetCharacter.GetPublicState<ICharacterPublicState>();
      var targetCurrentStats = targetPublicState.CurrentStats;

      // Clamp the max receivable damage to x5 from the max health.
      // This will help in case when the too much damage is dealt (mega-bomb!) 
      // to ensure the equipment will not receive excessive damaged.
      totalDamage = Math.Min(totalDamage, 5 * targetCurrentStats.HealthMax);

      // apply damage
      if (attackerCharacter is not null)
      {
        targetCurrentStats.ServerReduceHealth(totalDamage, damageSource: attackerCharacter);
      }
      else if (weaponCache.ProtoExplosive is not null)
      {
        targetCurrentStats.ServerReduceHealth(totalDamage, damageSource: weaponCache.ProtoExplosive);
      }
      else
      {
        // unknown damage - could be a vehicle or turret explosion, etc 
        Api.Logger.Warning("Unknown damage kind, unable to add it to damage tracking stats", targetCharacter);
        targetCurrentStats.ServerReduceHealth(totalDamage, damageSource: (IProtoGameObject)null);
      }

      Api.Logger.Info(
          $"Damage applied to {targetCharacter} by {attackerCharacter}:\n{totalDamage} dmg, current health {targetCurrentStats.HealthCurrent}/{targetCurrentStats.HealthMax}, {weaponCache.Weapon}");

      if (targetCurrentStats.HealthCurrent <= 0)
      {
        Api.Logger.Important(
            $"Character killed: {targetCharacter} by {attackerCharacter} with {weaponCache.Weapon?.ToString() ?? weaponCache.ProtoWeapon?.ToString()}");
        // no need to call it here as it's called automatically from ServerReduceHealth method
        //ServerCharacterDeathMechanic.OnCharacterKilled(attackerCharacter, targetCharacter);
      }

      damageApplied = totalDamage;
      ServerApplyDamageToEquippedItems(targetCharacter, damageApplied);
    }

    private static void ServerApplyDamageToEquippedItems(ICharacter targetCharacter, double damageApplied)
    {
      if (targetCharacter.IsNpc)
      {
        return;
      }

      // reduce equipped items durability
      // we're using temporary list here to prevent issues
      // when an item is destroyed during enumeration
      using var tempList = Api.Shared.WrapInTempList(targetCharacter.SharedGetPlayerContainerEquipment().Items);
      foreach (var item in tempList.AsList())
      {
        if (!(item.ProtoItem is IProtoItemWithDurability protoItemWithDurability))
        {
          continue;
        }

        try
        {
          protoItemWithDurability.ServerOnItemDamaged(item, damageApplied);
        }
        catch (Exception ex)
        {
          Api.Logger.Exception(ex);
        }
      }
    }

    private static double ServerCalculateDamageComponent(
        double damageValue,
        double invertedArmorPiercingCoef,
        DamageProportion damageProportion,
        double defenseFraction)
    {
      if (defenseFraction >= ObjectDefensePresets.Max)
      {
        // non-damageable defense
        return 0;
      }

      // decrease defense according to the armor piercing coefficient
      defenseFraction *= invertedArmorPiercingCoef;

      // calculate resistance parameters
      var resistanceValue = defenseFraction * 10;
      var resistanceFraction = MathHelper.Clamp(defenseFraction, 0, 1) * 0.75;

      // subtract resistance absolute value
      var damage = damageValue - resistanceValue;
      if (damage <= 0)
      {
        // damage is completely blocked by the armor
        return 0;
      }

      // subtract resistance fraction value
      damage *= 1 - resistanceFraction;

      // multiply on the damage proportion for current damage type
      damage *= damageProportion.Proportion;

      return damage;
    }

    private static double ServerCalculateTotalDamageByExplosive(
        ICharacter byCharacter,
        IProtoExplosive protoObjectExplosive,
        IStaticWorldObject targetStaticWorldObject,
        double damagePreMultiplier)
    {
      var damage = protoObjectExplosive.ServerCalculateTotalDamageByExplosive(byCharacter,
          targetStaticWorldObject);
      damage *= damagePreMultiplier;
      return damage;
    }

    private static bool ServerIsRestrictedPvPDamage(
        WeaponFinalCache weaponCache,
        IWorldObject targetObject,
        out bool isPvPcase,
        out bool isFriendlyFireCase)
    {
      isFriendlyFireCase = false;
      var damagingCharacter = weaponCache.Character;
      var targetCharacter = targetObject as ICharacter;

      if (targetCharacter is null
          && targetObject.ProtoGameObject is IProtoVehicle)
      {
        // the target object is vehicle so assume the target object is the vehicle's pilot
        targetCharacter = targetObject.GetPublicState<VehiclePublicState>()
                                      .PilotCharacter;
      }

      // Determine whether this is a PvP case.
      // Please note: turrets are also assumed as a PvP case!
      isPvPcase = targetCharacter is not null
                  && !(targetCharacter.IsNpc && targetCharacter.ProtoGameObject is IProtoCharacterMob)
                  && damagingCharacter is not null
                  && !(damagingCharacter.IsNpc && damagingCharacter.ProtoGameObject is IProtoCharacterMob);

      if (!isPvPcase)
      {
        // not a PvP damage so damage cannot be restricted
        return false;
      }

      // we have a PvP case when damage is dealt by weapon
      if (ReferenceEquals(targetCharacter, damagingCharacter)
          && weaponCache.ProtoExplosive is null)
      {
        // PvP damage disabled as player should not be able to harm self with a non-explosive weapon
        return true;
      }

      if (WeaponConstants.DamagePvpMultiplier <= 0)
      {
        // PvP damage disabled
        return true;
      }

      if (!PveSystem.SharedAllowPvPDamage(targetCharacter, damagingCharacter))
      {
        // no PvP damage allowed by PvE system
        return true;
      }

      if (WeaponConstants.DamageFriendlyFireMultiplier < 1.0)
      {
        // friendly fire enabled
        isFriendlyFireCase = ServerIsFriendlyFire(damagingCharacter, targetCharacter);
        if (isFriendlyFireCase)
        {
          // a friendly fire case
          // the damage is restricted only when the multiplier is zero
          return WeaponConstants.DamageFriendlyFireMultiplier == 0;
        }
      }

      return false;
    }
  }
}