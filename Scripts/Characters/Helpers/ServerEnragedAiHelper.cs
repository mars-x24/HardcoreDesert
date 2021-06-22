﻿namespace AtomicTorch.CBND.CoreMod.Characters
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Items.Weapons;
  using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
  using AtomicTorch.CBND.CoreMod.Systems.Physics;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.CoreMod.Vehicles;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Physics;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesServer;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;

  public static class ServerEnragedAiHelper
  {
    private static readonly IWorldServerService ServerWorldService
        = Api.IsServer
              ? Api.Server.World
              : null;

    private static readonly List<ICharacter> TempListPlayersInView
        = new();

    public static bool AnyTileObstaclesBetween(ICharacter npc, ICharacter player)
    {
      var physicsSpace = npc.PhysicsBody.PhysicsSpace;
      var npcCharacterCenter = npc.Position + npc.PhysicsBody.CenterOffset;
      var playerCharacterCenter = player.Position + player.PhysicsBody.CenterOffset;

      using var obstaclesInTheWay = physicsSpace.TestLine(
          npcCharacterCenter,
          playerCharacterCenter,
          CollisionGroup.Default,
          sendDebugEvent: false);

      foreach (var test in obstaclesInTheWay.AsList())
      {
        var testPhysicsBody = test.PhysicsBody;
        if (testPhysicsBody.AssociatedProtoTile is null)
        {
          continue;
        }

        var tile = ServerWorldService.GetTile(testPhysicsBody.Position.ToVector2Ushort());
        if (!tile.IsSlope)
        {
          // cliff tile on the way
          return true;
        }
      }

      return false;
    }
    public static bool AnyTileObstaclesBetween(ICharacter npc, IStaticWorldObject worldObject)
    {
      var physicsSpace = npc.PhysicsBody.PhysicsSpace;
      var npcCharacterCenter = npc.Position + npc.PhysicsBody.CenterOffset;
      var playerCharacterCenter = worldObject.TilePosition.ToVector2D() + worldObject.PhysicsBody.CenterOffset;

      using var obstaclesInTheWay = physicsSpace.TestLine(
          npcCharacterCenter,
          playerCharacterCenter,
          CollisionGroup.Default,
          sendDebugEvent: false);

      foreach (var test in obstaclesInTheWay.AsList())
      {
        var testPhysicsBody = test.PhysicsBody;
        if (testPhysicsBody.AssociatedProtoTile is null)
        {
          continue;
        }

        var tile = ServerWorldService.GetTile(testPhysicsBody.Position.ToVector2Ushort());
        if (!tile.IsSlope)
        {
          // cliff tile on the way
          return true;
        }
      }

      return false;
    }


    public static void CalculateDistanceAndDirectionToEnemy(
        ICharacter characterNpc,
        ICharacter enemyCharacter,
        bool isRangedWeapon,
        out double distanceToEnemy,
        out Vector2F directionToEnemyPosition,
        out Vector2F directionToEnemyHitbox)
    {
      if (enemyCharacter is null)
      {
        distanceToEnemy = double.NaN;
        directionToEnemyPosition = directionToEnemyHitbox = Vector2F.Zero;
        return;
      }

      var enemyWeaponOffset = isRangedWeapon
                                  ? enemyCharacter.ProtoCharacter.CharacterWorldWeaponOffsetRanged
                                  : enemyCharacter.ProtoCharacter.CharacterWorldWeaponOffsetMelee;

      var npcWeaponOffset = isRangedWeapon
                                ? characterNpc.ProtoCharacter.CharacterWorldWeaponOffsetRanged
                                : characterNpc.ProtoCharacter.CharacterWorldWeaponOffsetMelee;

      var deltaPos = enemyCharacter.Position - characterNpc.Position;
      directionToEnemyPosition = (Vector2F)deltaPos;

      deltaPos = (deltaPos.X, deltaPos.Y + enemyWeaponOffset - npcWeaponOffset);
      distanceToEnemy = deltaPos.Length;
      directionToEnemyHitbox = (Vector2F)deltaPos;
    }

    public static void CalculateDistanceAndDirectionToStructure(
        ICharacter characterNpc,
        IStaticWorldObject enemyStructure,
        bool isRangedWeapon,
        out double distanceToEnemy,
        out Vector2F directionToEnemyPosition,
        out Vector2F directionToEnemyHitbox)
    {
      if (enemyStructure is null)
      {
        distanceToEnemy = double.NaN;
        directionToEnemyPosition = directionToEnemyHitbox = Vector2F.Zero;
        return;
      }

      var npcWeaponOffset = isRangedWeapon
                                ? characterNpc.ProtoCharacter.CharacterWorldWeaponOffsetRanged
                                : characterNpc.ProtoCharacter.CharacterWorldWeaponOffsetMelee;

      var deltaPos = enemyStructure.TilePosition.ToVector2D() + enemyStructure.PhysicsBody.CenterOffset - characterNpc.Position;
     
      directionToEnemyPosition = (Vector2F)deltaPos;

      deltaPos = (deltaPos.X, deltaPos.Y - npcWeaponOffset);
      distanceToEnemy = deltaPos.Length;
      directionToEnemyHitbox = (Vector2F)deltaPos;
    }

    public static bool CanHitAnyTargetWithRangedWeapon(
        ICharacter characterNpc,
        double rotationAngleRad,
        CharacterMobPrivateState privateState,
        Func<IWorldObject, bool> isValidTargetCallback)
    {
      WeaponSystem.SharedCastLine(characterNpc,
                                  isMeleeWeapon: false,
                                  rangeMax: privateState.AttackRange,
                                  rotationAngleRad,
                                  customTargetPosition: null,
                                  fireSpreadAngleOffsetDeg: 0,
                                  CollisionGroups.HitboxRanged,
                                  out _,
                                  out var tempLineTestResults,
                                  sendDebugEvent: false);

      var hasObstacles = true;
      using (tempLineTestResults)
      {
        var characterTileHeight = characterNpc.Tile.Height;
        foreach (var testResult in tempLineTestResults.AsList())
        {
          var testResultPhysicsBody = testResult.PhysicsBody;

          var attackedProtoTile = testResultPhysicsBody.AssociatedProtoTile;
          if (attackedProtoTile is not null)
          {
            if (attackedProtoTile.Kind != TileKind.Solid)
            {
              // non-solid obstacle - skip
              continue;
            }

            var attackedTile = ServerWorldService.GetTile((Vector2Ushort)testResultPhysicsBody.Position);
            if (attackedTile.Height < characterTileHeight)
            {
              // attacked tile is below - ignore it
              continue;
            }

            // tile on the way - blocking damage ray
            break;
          }

          var damagedObject = testResultPhysicsBody.AssociatedWorldObject;
          if (ReferenceEquals(damagedObject, characterNpc))
          {
            // ignore collision with self
            continue;
          }

          if (damagedObject.ProtoGameObject is not IDamageableProtoWorldObject protoDamageableWorldObject)
          {
            // shoot through this object
            continue;
          }

          if (protoDamageableWorldObject.ObstacleBlockDamageCoef < 1
              && protoDamageableWorldObject is not IProtoCharacter)
          {
            // shoot through this object
            continue;
          }

          // the weapon system doesn't allow damage is there is no direct line of sight
          // on physical colliders layer between the two objects
          if (WeaponSystem.SharedHasTileObstacle(characterNpc.Position,
                                                 characterTileHeight,
                                                 damagedObject,
                                                 targetPosition: testResult.PhysicsBody.Position
                                                                 + testResult.PhysicsBody.CenterOffset))
          {
            continue;
          }

          // can hit
          var isValidTarget = isValidTargetCallback(damagedObject);
          hasObstacles = !isValidTarget;
          break;
        }
      }

      return !hasObstacles;
    }

    public static ICharacter GetClosestTargetPlayer(ICharacter characterNpc)
    {
      byte? tileHeight = null;

      try
      {
        var playersInView = TempListPlayersInView;
        ServerWorldService.GetCharactersInView(characterNpc,
                                               playersInView,
                                               onlyPlayerCharacters: true);
        if (playersInView.Count == 0)
        {
          return null;
        }

        ICharacter enemy = null;
        foreach (var playerCharacter in playersInView)
        {
          if (playerCharacter.GetPublicState<ICharacterPublicState>().IsDead)
          {
            // do not pay attention to dead characters
            continue;
          }

          if (playerCharacter.ProtoCharacter.GetType() != typeof(PlayerCharacter))
          {
            // don't react on non-player prototype (spectator?)
            continue;
          }

          var playerCharacterTile = playerCharacter.Tile;
          if (!tileHeight.HasValue)
          {
            var npcTile = characterNpc.Tile;
            tileHeight = npcTile.Height;
          }

          if (playerCharacterTile.Height != tileHeight.Value)
          {
            // attack only on the same height characters
            // unless there is a direct line of sight between the NPC and the target
            if (AnyTileObstaclesBetween(characterNpc, playerCharacter))
            {
              continue;
            }
          }

          enemy = playerCharacter;
          break;
        }

        return enemy;
      }
      finally
      {
        TempListPlayersInView.Clear();
      }
    }

    public static IStaticWorldObject GetClosestTargetStructure(ICharacter characterNpc)
    {
      byte? tileHeight = null;

      var list = ServerWorldService.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectStructure>(
        new RectangleInt(characterNpc.TilePosition.X, characterNpc.TilePosition.Y, 1, 1).Inflate(25))
        .Where(S => S.PhysicsBody.HasAnyShapeCollidingWithGroup(CollisionGroups.HitboxMelee))
        .OrderBy(S => characterNpc.Position.DistanceTo(S.TilePosition.ToVector2D())).ToList();

      if (list.Count == 0)
      {
        return null;
      }

      IStaticWorldObject structure = null;
      foreach (var worldObject in list)
      {
        if (!(worldObject.ProtoGameObject is IDamageableProtoWorldObject))
          continue;

        var tile = worldObject.OccupiedTile;
        if (!tileHeight.HasValue)
        {
          var npcTile = characterNpc.Tile;
          tileHeight = npcTile.Height;
        }

        if (tile.Height != tileHeight.Value)
        {
          // attack only on the same height
          // unless there is a direct line of sight between the NPC and the target
          if (AnyTileObstaclesBetween(characterNpc, worldObject))
          {
            continue;
          }
        }

        structure = worldObject;
        break;
      }

      return structure;
    }

    public static void LookOnEnemy(Vector2F directionToEnemyHitbox, ref double rotationAngleRad)
    {
      if (directionToEnemyHitbox != Vector2F.Zero)
      {
        rotationAngleRad = Math.Abs(
            Math.Atan2(directionToEnemyHitbox.Y, directionToEnemyHitbox.X) + 2 * Math.PI);
      }
    }

    public static void ProcessAggressiveAi(
        ICharacter characterNpc,
        IStaticWorldObject targetStructure,
        ICharacter targetCharacter,
        double distanceEnemyTooClose,
        double distanceEnemyTooFar,
        out Vector2F movementDirection,
        out double rotationAngleRad,
        IReadOnlyList<AiWeaponPreset> weaponList = null,
        bool attackFarOnlyIfAggro = false,
        Func<IWorldObject, bool> customIsValidTargetCallback = null)
    {
      var privateState = characterNpc.GetPrivateState<CharacterMobEnragedPrivateState>();
      var weaponState = privateState.WeaponState;

      var lastTargetCharacter = privateState.CurrentTargetCharacter;
      var lastTargetStructure = privateState.CurrentTargetStructure;

      var isRangedWeapon = weaponState.ProtoWeapon is IProtoItemWeaponRanged
                           || weaponState.ProtoWeapon is ProtoItemMobWeaponRangedNoAim;

      CalculateDistanceAndDirectionToStructure(characterNpc,
                                     targetStructure,
                                     isRangedWeapon: isRangedWeapon,
                                     out var distanceToTargetStructure,
                                     out var directionToEnemyPositionStructure,
                                     out var directionToEnemyHitboxStructure);

      CalculateDistanceAndDirectionToEnemy(characterNpc,
                                           targetCharacter,
                                           isRangedWeapon: isRangedWeapon,
                                           out var distanceToTarget,
                                           out var directionToEnemyPosition,
                                           out var directionToEnemyHitbox);

      //if(distanceToTargetStructure < distanceToTarget || double.IsNaN(distanceToTarget))
      //{
      //  distanceToTarget = distanceToTargetStructure;
      //  directionToEnemyPosition = directionToEnemyPositionStructure;
      //  directionToEnemyHitbox = directionToEnemyHitboxStructure;

      //  targetCharacter = null;
      //}

      if(targetCharacter is null)
      {
        distanceToTarget = distanceToTargetStructure;
        directionToEnemyPosition = directionToEnemyPositionStructure;
        directionToEnemyHitbox = directionToEnemyHitboxStructure;
      }

      if (targetCharacter is not null && ReferenceEquals(targetCharacter, privateState.CurrentAggroCharacter))
      {
        // increase distances if aggro on this character
        distanceEnemyTooFar *= 3;
      }
    
      var isTargetTooFar = distanceToTarget > distanceEnemyTooFar;
      movementDirection = distanceToTarget < distanceEnemyTooClose
                          || isTargetTooFar
                              ? Vector2F.Zero // too close or too far
                              : directionToEnemyPosition;

      if (isTargetTooFar)
      {
        targetCharacter = null;
        targetStructure = null;
      }

      privateState.CurrentTargetCharacter = targetCharacter;
      privateState.CurrentTargetStructure = targetStructure;

      rotationAngleRad = characterNpc.GetPublicState<CharacterMobPublicState>()
                                     .AppliedInput
                                     .RotationAngleRad;

      LookOnEnemy(directionToEnemyHitbox, ref rotationAngleRad);

      var isAttacking = false;

      if (!double.IsNaN(distanceToTarget))
      {
        if (weaponList is null)
        {
          isAttacking = distanceToTarget <= privateState.AttackRange;
        }
        else
        {
          SelectAiWeapon(characterNpc,
                         distanceToTarget,
                         weaponList,
                         out var desiredProtoWeapon,
                         out var isWithinRange);

          isAttacking = isWithinRange
                        && ReferenceEquals(desiredProtoWeapon,
                                           weaponState.ProtoWeapon);
        }

        if (isAttacking
            && attackFarOnlyIfAggro
            && distanceToTarget > distanceEnemyTooFar
            && (privateState.CurrentAggroCharacter is null
                || !ReferenceEquals(privateState.CurrentAggroCharacter, targetCharacter)))
        {
          // don't attack a non-aggro target that is far
          isAttacking = false;
        }

        if (isRangedWeapon
            && isAttacking)
        {
          // do not attack with a ranged weapon if there is no direct visibility with the target
          isAttacking = CanHitAnyTargetWithRangedWeapon(
              characterNpc,
              rotationAngleRad,
              privateState,
              isValidTargetCallback: customIsValidTargetCallback ?? IsValidTargetCallback);

          static bool IsValidTargetCallback(IWorldObject worldObject)
              => worldObject is ICharacter { IsNpc: false }
                 || worldObject.ProtoGameObject is IProtoVehicle;
        }

        var currentPosition = characterNpc.Position;
        if (!isAttacking 
          && Math.Round(privateState.LastPosition.X, 1) == Math.Round(currentPosition.X, 1)
          && Math.Round(privateState.LastPosition.Y, 1) == Math.Round(currentPosition.Y, 1))
        {
          //I am stuck, attack something
          if (movementDirection != Vector2F.Zero && privateState.CurrentTargetCharacter != null)
          {
            isAttacking = true;
          }
        }

        privateState.LastPosition = currentPosition;
      }

      weaponState.SharedSetInputIsFiring(isAttacking);

      if (targetCharacter is null && targetStructure is null)
      {
        return;
      }

      // not retreating
      if (lastTargetCharacter != targetCharacter)
      {
        // changed an enemy
        if (characterNpc.ProtoCharacter is IProtoCharacterMob protoMob)
        {
          protoMob.ServerPlaySound(characterNpc, CharacterSound.Aggression);
        }
      }
    }

    private static void SelectAiWeapon(
        ICharacter character,
        double distanceToTarget,
        IReadOnlyList<AiWeaponPreset> weaponList,
        out IProtoItemWeapon desiredProtoWeapon,
        out bool isWithinRange)
    {
      desiredProtoWeapon = null;
      isWithinRange = false;

      var privateState = character.GetPrivateState<CharacterMobPrivateState>();
      var weaponState = privateState.WeaponState;
      if (weaponState.CooldownSecondsRemains > 0.001
          || weaponState.DamageApplyDelaySecondsRemains > 0.001)
      {
        // cannot switch weapon now, try to use the currently selected weapon
        //Api.Logger.Dev("Weapon cooldown remains: " + weaponState.CooldownSecondsRemains
        //               + " damageApplyDelaySecondsRemains: " +  weaponState.DamageApplyDelaySecondsRemains);
        desiredProtoWeapon = weaponState.ProtoWeapon;
        foreach (var weaponPreset in weaponList)
        {
          if (ReferenceEquals(weaponPreset.ProtoWeapon, desiredProtoWeapon))
          {
            isWithinRange = distanceToTarget < weaponPreset.MaxAttackRange;
            break;
          }
        }

        return;
      }

      // try to select the weapon from the list
      foreach (var weaponPreset in weaponList)
      {
        isWithinRange = distanceToTarget < weaponPreset.MaxAttackRange;
        if (!isWithinRange)
        {
          continue;
        }

        desiredProtoWeapon = weaponPreset.ProtoWeapon;
        ServerMobWeaponHelper.TrySetWeapon(character,
                                           desiredProtoWeapon,
                                           rebuildWeaponsCacheNow: true);
        break;
      }
    }
  }
}