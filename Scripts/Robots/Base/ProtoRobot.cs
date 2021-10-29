namespace AtomicTorch.CBND.CoreMod.Robots
{
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Helpers.Client;
  using AtomicTorch.CBND.CoreMod.ItemContainers;
  using AtomicTorch.CBND.CoreMod.Items.Robots;
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
  using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
  using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
  using AtomicTorch.CBND.CoreMod.Systems.Notifications;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.Physics;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using AtomicTorch.GameEngine.Common.Primitives;
  using HardcoreDesert.Scripts.Robots.Base;
  using System;
  using System.Linq;
  using System.Windows.Media;

  public abstract class ProtoRobot
        <TItemRobot,
         TPrivateState,
         TPublicState,
         TClientState>
        : ProtoDynamicWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoRobot
        where TItemRobot : IProtoItemRobot, new()
        where TPrivateState : RobotPrivateState, new()
        where TPublicState : RobotPublicState, new()
        where TClientState : BaseClientState, new()
  {
    private static readonly double DistanceThresholdToStructure = 0.15;

    private static readonly double DistanceThresholdToOwner = 0.3;

    private static readonly Lazy<TItemRobot> LazyProtoItemRobot
        = new(Api.GetProtoEntity<TItemRobot>);

    private double lastRobotReturnSoundTime;

    private double lastRobotStartSoundTime;

    protected ProtoRobot()
    {
      var name = this.GetType().Name;
      if (!name.StartsWith("Robot", StringComparison.Ordinal))
      {
        throw new Exception("Robot class name must start with \"Robot\": " + this.GetType().Name);
      }

      this.ShortId = name.Substring("Robot".Length);
      // ReSharper disable once VirtualMemberCallInConstructor
      this.DefaultTextureResource = this.CreateDefaultTexture();
    }

    public override double ClientUpdateIntervalSeconds => 1;

    public virtual ITextureResource DefaultTextureResource { get; }

    public virtual float DestroyedExplosionVolume => 1;

    public DamageDescription DestroyedVehicleDamageDescriptionCharacters { get; private set; }

    public double DestroyedVehicleDamageRadius { get; private set; }

    public sealed override string Name => LazyProtoItemRobot.Value.Name;

    public abstract double PhysicsBodyAccelerationCoef { get; }

    public abstract double PhysicsBodyFriction { get; }

    public IProtoItemRobot ProtoItemRobot => LazyProtoItemRobot.Value;

    public override double ServerUpdateIntervalSeconds => 0.1;

    public override string ShortId { get; }

    public abstract double StatMoveSpeed { get; }

    protected ExplosionPreset DestroyedExplosionPreset { get; private set; }

    protected virtual byte DestroyedExplosionRadius => 30;

    protected abstract double DrawVerticalOffset { get; }

    protected virtual SoundResource RobotReturnOrDropSoundResource { get; }
        = new("Items/Robots/Return");

    protected virtual SoundResource RobotStartSoundResource { get; }
        = new("Items/Robots/Start");

    protected abstract SoundResource EngineSoundResource { get; }

    protected abstract double EngineSoundVolume { get; }

    protected virtual double ShadowOpacity => 0.5;

    public virtual byte ItemDeliveryCount => 1;

    public void ServerDropRobotToGround(
        IDynamicWorldObject objectRobot,
        Tile tile,
        ICharacter forOwnerCharacter)
    {
      var privateState = objectRobot.GetPrivateState<RobotPrivateState>();
      var storageContainer = privateState.StorageItemsContainer;
      if (storageContainer.OccupiedSlotsCount == 0)
      {
        return;
      }

      // drop all items on the ground
      IItemsContainer groundContainer = null;
      if (forOwnerCharacter is not null
          && storageContainer.Items.Contains(privateState.AssociatedItem))
      {
        // a robot item will be dropped as well,
        // use a loot container to have a map mark 
        groundContainer = ObjectPlayerLootContainer.ServerTryCreateLootContainer(
            forOwnerCharacter,
            tile.Position.ToVector2D() + (0.5, 0.5));

        if (groundContainer is not null)
        {
          // set slots count matching the total occupied slots count
          Server.Items.SetSlotsCount(
              groundContainer,
              (byte)Math.Min(byte.MaxValue,
                             groundContainer.OccupiedSlotsCount
                             + storageContainer.OccupiedSlotsCount));
        }
      }

      groundContainer ??=
          ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(
              forOwnerCharacter,
              tile);

      if (groundContainer is null)
      {
        // no space around the player (extremely rare case)
        // TODO: try to create a ground container in any other ground spot
        Logger.Error("Cannot find a place to drop the robot contents on the ground - robot lost! "
                     + objectRobot);
        return;
      }

      Server.Items.TryMoveAllItems(storageContainer, groundContainer);

      WorldObjectClaimSystem.ServerTryClaim(
          groundContainer.OwnerAsStaticObject,
          forOwnerCharacter,
          durationSeconds: groundContainer.OwnerAsStaticObject.ProtoStaticWorldObject
                               is ObjectPlayerLootContainer
                               ? ObjectPlayerLootContainer.AutoDestroyTimeoutSeconds + (10 * 60)
                               : WorldObjectClaimDuration.DroppedGoods);

      if (forOwnerCharacter is not null
          && groundContainer.OccupiedSlotsCount > 0)
      {
        if (!objectRobot.IsDestroyed)
        {
          NotificationSystem.ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
              forOwnerCharacter,
              protoItemForIcon: groundContainer.Items.First().ProtoItem);
        }
      }

      if (storageContainer.OccupiedSlotsCount == 0)
      {
        return;
      }

      Logger.Error("Not all items dropped on the ground from the robot storage: "
                   + objectRobot
                   + " slots occupied: "
                   + storageContainer.OccupiedSlotsCount);
    }

    public IItemsContainer ServerGetStorageItemsContainer(IDynamicWorldObject objectRobot)
    {
      return GetPrivateState(objectRobot).StorageItemsContainer;
    }

    public override void ServerOnDestroy(IDynamicWorldObject gameObject)
    {
      base.ServerOnDestroy(gameObject);

      var droneTile = gameObject.Tile;

      var privateState = gameObject.GetPrivateState<RobotPrivateState>();
      var owner = privateState.Owner;

      var droneItem = privateState.AssociatedItem;
      if (droneItem is not null)
      {
        Server.Items.DestroyItem(droneItem);
      }

      if (privateState.AssociatedItemReservedSlot is not null)
      {
        Server.Items.DestroyItem(privateState.AssociatedItemReservedSlot);
      }

      try
      {
        var protoRobot = (IProtoRobot)gameObject.ProtoGameObject;
        protoRobot.ServerDropRobotToGround(gameObject, droneTile, owner is ICharacter character ? character : null);
      }
      catch (Exception ex)
      {
        Logger.Exception(ex);
      }

      ServerTimersSystem.AddAction(
          0,
          () =>
          {
            Server.Items.DestroyContainer(privateState.ReservedItemsContainer);
            Server.Items.DestroyContainer(privateState.StorageItemsContainer);
          });

    }

    public void ServerOnRobotDroppedOrReturned(
        IDynamicWorldObject objectRobot,
        ICharacter toCharacter,
        bool isReturnedToPlayer)
    {
      if (!isReturnedToPlayer)
      {
        toCharacter = null;
      }

      var privateState = GetPrivateState(objectRobot);
      var itemReservedSlot = privateState.AssociatedItemReservedSlot;
      if (itemReservedSlot is not null
          && !itemReservedSlot.IsDestroyed
          && itemReservedSlot.Container != privateState.ReservedItemsContainer)
      {
        Server.Items.MoveOrSwapItem(itemReservedSlot,
                                    privateState.ReservedItemsContainer,
                                    movedCount: out _);
      }

      using var observers = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(objectRobot, observers);

      this.CallClient(observers.AsList(),
                      _ => _.ClientRemote_OnRobotDroppedOrReturned(toCharacter,
                                                                   objectRobot.TilePosition,
                                                                   isReturnedToPlayer));
    }

    public void ServerSetupAssociatedItem(
        IDynamicWorldObject objectRobot,
        IItem item)
    {
      var robotPrivateState = GetPrivateState(objectRobot);
      robotPrivateState.AssociatedItem = item;
    }

    public void ServerStartRobot(
        IDynamicWorldObject objectRobot,
        IWorldObject owner,
        IItemsContainer ownerContainer)
    {
      var privateState = GetPrivateState(objectRobot);
      if (!privateState.IsDespawned)
        return;

      var itemRobot = privateState.AssociatedItem;

      // move robot from owner inventory to its own storage items container
      var fromSlotIndex = itemRobot.ContainerSlotId;
      Server.Items.MoveOrSwapItem(itemRobot,
                                  this.ServerGetStorageItemsContainer(objectRobot),
                                  out ushort robotMovedCount);

      // move reserved slot item into the slot where the robot was located previously
      ServerCreateReservedSlotItemIfNecessary(privateState, objectRobot);
      Server.Items.MoveOrSwapItem(privateState.AssociatedItemReservedSlot,
                                  ownerContainer,
                                  out ushort movedCount,
                                  slotId: fromSlotIndex);

      // ensure the robot is the first item in the storage container
      if (itemRobot.ContainerSlotId != 0)
      {
        Server.Items.MoveOrSwapItem(itemRobot,
                                    privateState.StorageItemsContainer,
                                    out _,
                                    slotId: 0);
      }

      privateState.Owner = owner;
      privateState.OwnerContainer = ownerContainer;
      privateState.IsDespawned = false;
      objectRobot.ProtoGameObject.ServerSetUpdateRate(objectRobot, isRare: false);

      var position = owner.TilePosition.ToVector2D() + RobotTargetPositionHelper.GetTargetPosition(owner);

      Server.World.SetPosition(objectRobot, position, writeToLog: false);
      // recreate physics (as spawned robot has physics)
      objectRobot.ProtoWorldObject.SharedCreatePhysics(objectRobot);

      using var observers = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetScopedByPlayers(objectRobot, observers);
      this.CallClient(observers.AsList(),
                      _ => _.ClientRemote_OnRobotStart(objectRobot));
    }

    protected virtual IComponentSpriteRenderer ClientCreateRendererShadow(
        IDynamicWorldObject worldObject,
        double scaleMultiplier)
    {
      var rendererShadow = ClientShadowHelper.AddShadowRenderer(
          worldObject,
          scaleMultiplier: (float)scaleMultiplier);

      if (rendererShadow is null)
      {
        return null;
      }

      rendererShadow.PositionOffset = default;
      rendererShadow.Color = Color.FromArgb((byte)(byte.MaxValue * this.ShadowOpacity), 0x00, 0x00, 0x00);
      rendererShadow.DrawOrder = DrawOrder.Shadow;
      return rendererShadow;
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      var objectRobot = data.GameObject;

      var spriteRenderer = this.ClientSetupRendering(data);
      this.ClientCreateRendererShadow(data.GameObject, scaleMultiplier: 1.0);
      this.ClientSetupSoundEmitter(data);
      this.ClientSetupHealthbar(data);

      var sceneObject = objectRobot.ClientSceneObject;
      sceneObject.AddComponent<ComponentRobotVisualManager>()
                 .Setup(objectRobot,
                        spriteRenderer,
                        this.StatMoveSpeed);
    }

    protected virtual IComponentSpriteRenderer ClientSetupRendering(ClientInitializeData data)
    {
      var worldObject = data.GameObject;
      var spriteRenderer = Client.Rendering.CreateSpriteRenderer(
          worldObject,
          this.DefaultTextureResource,
          spritePivotPoint: (0.5, 0.5));
      spriteRenderer.Scale = 0.75;
      spriteRenderer.PositionOffset = (0, this.DrawVerticalOffset);
      return spriteRenderer;
    }

    protected virtual IComponentSoundEmitter ClientSetupSoundEmitter(ClientInitializeData data)
    {
      return Client.Audio.CreateSoundEmitter(
          data.GameObject,
          this.EngineSoundResource,
          is3D: true,
          isLooped: true,
          radius: this.ObjectSoundRadius,
          volume: (float)this.EngineSoundVolume,
          isPlaying: true);
    }

    protected virtual ITextureResource CreateDefaultTexture()
    {
      return new TextureResource("Robots/" + this.GetType().Name);
    }

    protected sealed override void PrepareProtoDynamicWorldObject()
    {
      base.PrepareProtoDynamicWorldObject();

      this.PrepareProtoVehicleDestroyedExplosionPreset(
          out var destroyedVehicleDamageRadius,
          out var destroyedExplosionPreset,
          out var destroyedVehicleDamageDescriptionCharacters);

      this.DestroyedExplosionPreset = destroyedExplosionPreset
                                      ?? throw new Exception("No explosion preset provided");

      this.DestroyedVehicleDamageRadius = destroyedVehicleDamageRadius;
      this.DestroyedVehicleDamageDescriptionCharacters = destroyedVehicleDamageDescriptionCharacters;
    }

    protected abstract void PrepareProtoVehicleDestroyedExplosionPreset(
        out double damageRadius,
        out ExplosionPreset explosionPreset,
        out DamageDescription damageDescriptionCharacters);

    protected virtual void ServerExecuteVehicleExplosion(
        Vector2D positionEpicenter,
        IPhysicsSpace physicsSpace,
        WeaponFinalCache weaponFinalCache)
    {
      WeaponExplosionSystem.ServerProcessExplosionCircle(
          positionEpicenter: positionEpicenter,
          physicsSpace: physicsSpace,
          damageDistanceMax: this.DestroyedVehicleDamageRadius,
          weaponFinalCache: weaponFinalCache,
          damageOnlyDynamicObjects: false,
          isDamageThroughObstacles: false,
          callbackCalculateDamageCoefByDistanceForStaticObjects: CalcDamageCoefByDistance,
          callbackCalculateDamageCoefByDistanceForDynamicObjects: CalcDamageCoefByDistance);

      double CalcDamageCoefByDistance(double distance)
      {
        var distanceThreshold = 0.5;
        if (distance <= distanceThreshold)
        {
          return 1;
        }

        distance -= distanceThreshold;
        distance = Math.Max(0, distance);

        var maxDistance = this.DestroyedVehicleDamageRadius;
        maxDistance -= distanceThreshold;
        maxDistance = Math.Max(0, maxDistance);

        return 1 - Math.Min(distance / maxDistance, 1);
      }
    }

    protected override void ServerInitialize(ServerInitializeData data)
    {
      var objectRobot = data.GameObject;
      base.ServerInitialize(data);

      data.PrivateState.StorageItemsContainer
          ??= Api.Server.Items.CreateContainer<ItemsContainerDefault>(objectRobot,
                                                                      slotsCount: 255);

      data.PrivateState.ReservedItemsContainer
          ??= Api.Server.Items.CreateContainer<ItemsContainerDefault>(objectRobot,
                                                                      slotsCount: 255);
    }

    protected override void ServerOnDynamicObjectZeroStructurePoints(
        WeaponFinalCache weaponCache,
        ICharacter byCharacter,
        IWorldObject targetObject)
    {
      var vehicle = (IDynamicWorldObject)targetObject;

      // explode
      using var scopedBy = Api.Shared.GetTempList<ICharacter>();
      Server.World.GetCharactersInRadius(vehicle.TilePosition,
                                         scopedBy,
                                         radius: this.DestroyedExplosionRadius,
                                         onlyPlayers: true);

      this.CallClient(scopedBy.AsList(),
                      _ => _.ClientRemote_VehicleExploded(vehicle.Position));

      SharedExplosionHelper.ServerExplode(
          character:
          null, // yes, no damaging character here otherwise it will not receive the damage if staying close
          protoExplosive: null,
          protoWeapon: null,
          explosionPreset: this.DestroyedExplosionPreset,
          epicenterPosition: vehicle.Position
                             + this.SharedGetObjectCenterWorldOffset(targetObject),
          damageDescriptionCharacters: this.DestroyedVehicleDamageDescriptionCharacters,
          physicsSpace: targetObject.PhysicsBody.PhysicsSpace,
          executeExplosionCallback: this.ServerExecuteVehicleExplosion);

      // destroy the vehicle completely after short explosion delay
      ServerTimersSystem.AddAction(
          this.DestroyedExplosionPreset.ServerDamageApplyDelay,
          () => base.ServerOnDynamicObjectZeroStructurePoints(weaponCache,
                                                              byCharacter,
                                                              targetObject));
    }

    protected override void ServerUpdate(ServerUpdateData data)
    {
      base.ServerUpdate(data);

      var objectRobot = data.GameObject;
      var privateState = data.PrivateState;
      var publicState = data.PublicState;

      if (privateState.AssociatedItem is null)
      {
        // incorrectly configured robot
        Server.World.DestroyObject(objectRobot);
        if (privateState.AssociatedItemReservedSlot is not null)
        {
          Server.Items.DestroyItem(privateState.AssociatedItemReservedSlot);
        }

        return;
      }

      ServerCreateReservedSlotItemIfNecessary(privateState, objectRobot);

      if (privateState.IsDespawned)
      {
        this.ServerSetUpdateRate(objectRobot, isRare: true);
        return;
      }

      Vector2D destinationCoordinate;

      var hasTarget = publicState.Target is not null && !publicState.IsGoingBackToOwner;

      if (hasTarget)
      {
        destinationCoordinate = publicState.Target.TilePosition.ToVector2D() + RobotTargetPositionHelper.GetTargetPosition(publicState.Target);

        if (!RobotTargetHelper.ServerPickupAllowed(publicState.TargetItems, objectRobot))
        {
          // cannot pick this item as it's already targeted by another robot
          publicState.ResetTargetPosition();
          return;
        }
      }
      else
      {
        // should return to the owner to despawn
        if (privateState.Owner is null)
          return;

        if (privateState.Owner is ICharacter character)
        {
          if (PlayerCharacter.GetPrivateState(character).IsDespawned
           || PlayerCharacter.GetPublicState(character).IsDead)
            return;
        }

        destinationCoordinate = privateState.Owner.TilePosition.ToVector2D() + RobotTargetPositionHelper.GetTargetPosition(privateState.Owner);
      }

      RefreshMovement(isToDelivery: hasTarget,
                      destinationCoordinate,
                      out var isDestinationReached);
      if (!isDestinationReached)
      {
        return;
      }

      if (hasTarget)
      {
        PickUpItem();

        publicState.IsGoingBackToOwner = true;
      }
      else
      {
        // we were going to the player and reached its location, despawn
        ServerOnRobotReturnedToOwner(objectRobot);
      }

      void RefreshMovement(
          bool isToDelivery,
          Vector2D toPosition,
          out bool isDestinationReached)
      {
        var positionDelta = toPosition - objectRobot.Position;
        var positionDeltaLength = positionDelta.Length;

        double targetVelocity;
        if (positionDeltaLength
            > (isToDelivery
                   ? DistanceThresholdToStructure
                   : DistanceThresholdToOwner))
        {
          // fly towards that object
          var moveSpeed = this.StatMoveSpeed;

          targetVelocity = moveSpeed;
          isDestinationReached = false;

          if (isToDelivery)
          {
            // reduce speed when too close to the target
            var distanceCoef = positionDeltaLength / (0.333 * targetVelocity);
            if (distanceCoef < 1)
            {
              targetVelocity *= Math.Pow(distanceCoef, 0.5);
              // ensure it cannot drop lower than 5% of the original move speed
              targetVelocity = Math.Max(0.05 * moveSpeed, targetVelocity);
            }
          }
        }
        else
        {
          isDestinationReached = true;

          // stop
          if (Server.World.GetDynamicObjectMoveSpeed(objectRobot) == 0)
          {
            // already stopped
            return;
          }

          targetVelocity = 0;
        }

        var movementDirectionNormalized = positionDelta.Normalized;
        var moveAcceleration = movementDirectionNormalized * this.PhysicsBodyAccelerationCoef * targetVelocity;

        Server.World.SetDynamicObjectMoveSpeed(objectRobot, targetVelocity);

        Server.World.SetDynamicObjectPhysicsMovement(objectRobot,
                                                     moveAcceleration,
                                                     targetVelocity: targetVelocity);
        objectRobot.PhysicsBody.Friction = this.PhysicsBodyFriction;
      }

      void PickUpItem()
      {
        var items = RobotTargetHelper.GetOutputContainersItems(publicState.Target);

        foreach (var item in publicState.TargetItems)
        {
          if (!items.Contains(item))
            continue;

          if (privateState.OwnerContainer is null || privateState.OwnerContainer.EmptySlotsCount == 0)
            return;

          Api.Server.Items.MoveOrSwapItem(item, privateState.StorageItemsContainer, out _);

          // reduce robot durability on 1 unit (reflected as HP when it's a world object)
          // but ensure the new HP cannot drop to exact 0 (to prevent destruction)
          var newHP = publicState.StructurePointsCurrent
                      - 1 * LazyProtoItemRobot.Value.DurabilityToStructurePointsConversionCoefficient;
          publicState.StructurePointsCurrent = Math.Max(float.Epsilon, (float)newHP);
        }
      }
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      if (IsServer
          && data.PrivateState.IsDespawned)
      {
        // no physics for despawned robots
        return;
      }

      this.SharedCreatePhysicsRobot(data);
    }

    protected abstract void SharedCreatePhysicsRobot(CreatePhysicsData data);

    private static void ServerCreateReservedSlotItem(IDynamicWorldObject objectRobot)
    {
      var privateState = GetPrivateState(objectRobot);
      var itemRobotReservedSlot = Server.Items.CreateItem(
                                            Api.GetProtoEntity<ItemRobotReservedSlot>(),
                                            privateState.ReservedItemsContainer)
                                        .ItemAmounts
                                        .First()
                                        .Key;

      privateState.AssociatedItemReservedSlot = itemRobotReservedSlot;
      ItemRobotReservedSlot.ServerSetup(itemRobotReservedSlot,
                                        (IProtoItemRobot)privateState.AssociatedItem.ProtoItem);
    }

    private static void ServerCreateReservedSlotItemIfNecessary(
        TPrivateState privateState,
        IDynamicWorldObject objectRobot)
    {
      if (privateState.AssociatedItemReservedSlot is null
          || privateState.AssociatedItemReservedSlot.IsDestroyed)
      {
        ServerCreateReservedSlotItem(objectRobot);
      }
    }

    private static void ServerOnRobotReturnedToOwner(IDynamicWorldObject objectRobot)
    {
      var privateState = GetPrivateState(objectRobot);
      var owner = privateState.Owner;
      var ownerContainer = privateState.OwnerContainer;
      var characterOwner = privateState.Owner as ICharacter;

      var tile = objectRobot.Tile;
      var storageContainer = privateState.StorageItemsContainer;
      if (storageContainer.OccupiedSlotsCount > 0)
        MoveContents();

      ServerDespawnRobot(objectRobot, isReturnedToOwner: true);

      RobotTargetHelper.ServerUnregisterCurrentPickup(objectRobot);

      void MoveContents()
      {
        // drop storage container contents to owner container
        // but first, move the robot item to its original slot (if possible)
        var itemReservedSlot = privateState.AssociatedItemReservedSlot;
        if (itemReservedSlot is not null
            && itemReservedSlot.IsDestroyed)
        {
          itemReservedSlot = null;
        }

        // ReSharper disable once PossibleNullReferenceException
        var reservedContainer = itemReservedSlot?.Container;
        var reservedContainerSlotId = itemReservedSlot?.ContainerSlotId ?? 0;
        if (itemReservedSlot is not null)
        {
          Server.Items.MoveOrSwapItem(itemReservedSlot,
                                      privateState.ReservedItemsContainer,
                                      movedCount: out _);
        }

        if (reservedContainer is not null
            && reservedContainer.Owner == owner
            && storageContainer.GetItemAtSlot(0) is { } itemRobot)
        {
          // Return the associated item (the robot item itself) to the reserved slot location.
          // (The item in the first slot is the robot's associated item.
          // It can be destroyed in the case when the robot's HP dropped <= 1
          // so we check whether the item in the first slot is not null)
          Server.Items.MoveOrSwapItem(itemRobot,
                                      reservedContainer,
                                      slotId: reservedContainerSlotId,
                                      movedCount: out _);
        }

        var result = Server.Items.TryMoveAllItems(storageContainer, ownerContainer);
        try
        {
          if (storageContainer.OccupiedSlotsCount == 0)
          {
            // all items moved from robot to player
            return;
          }

          if (storageContainer.OccupiedSlotsCount == 0)
          {
            // all items moved from robot to owner
            return;
          }
        }
        finally
        {
          if (result.MovedItems.Count > 0)
          {
            // notify player about the received items
            NotificationSystem.ServerSendItemsNotification(
                characterOwner,
                result.MovedItems
                      .GroupBy(p => p.Key.ProtoItem)
                      .Where(p => p.Key is not TItemRobot)
                      .ToDictionary(p => p.Key, p => p.Sum(v => v.Value)));
          }
        }

        // try to drop the remaining items on the ground
        ((IProtoRobot)objectRobot.ProtoGameObject)
            .ServerDropRobotToGround(objectRobot, tile, characterOwner);
      }
    }

    private static void ServerDespawnRobot(IDynamicWorldObject objectRobot, bool isReturnedToOwner)
    {
      if (objectRobot.IsDestroyed)
      {
        return;
      }

      var privateState = objectRobot.GetPrivateState<RobotPrivateState>();
      var publicState = objectRobot.GetPublicState<RobotPublicState>();

      publicState.ResetTargetPosition();
      if (privateState.IsDespawned)
      {
        return;
      }

      var robotItem = privateState.AssociatedItem;
      var protoItemRobot = (IProtoItemRobot)robotItem.ProtoItem;
      var world = Server.World;

      var characterOwner = privateState.Owner as ICharacter;

      var protoRobot = protoItemRobot.ProtoRobot;
      protoRobot.ServerOnRobotDroppedOrReturned(objectRobot, characterOwner, isReturnedToOwner);
      privateState.IsDespawned = true;
      privateState.Owner = null;
      privateState.OwnerContainer = null;

      // recreate physics (as despawned drone doesn't have any physics)
      world.StopPhysicsBody(objectRobot.PhysicsBody);
      objectRobot.ProtoWorldObject.SharedCreatePhysics(objectRobot);
      world.SetPosition(objectRobot,
                        CharacterDespawnSystem.ServerGetServiceAreaPosition().ToVector2D());

      try
      {
        DeductDurability();
      }
      catch (Exception ex)
      {
        Logger.Exception(ex);
      }

      void DeductDurability()
      {
        var currentDurability = (int)(objectRobot.GetPublicState<RobotPublicState>().StructurePointsCurrent
                                      / protoItemRobot.DurabilityToStructurePointsConversionCoefficient);
        if (currentDurability <= 1)
        {
          currentDurability = 0;
        }

        var deltaDurabilility = (int)(ItemDurabilitySystem.SharedGetDurabilityValue(robotItem)
                                      - currentDurability);

        if (deltaDurabilility <= 0)
        {
          return;
        }

        ItemDurabilitySystem.ServerModifyDurability(robotItem,
                                                    -deltaDurabilility);

        if (!robotItem.IsDestroyed)
        {
          return;
        }

        // drone item degraded to 100%, notify the player
        ItemDurabilitySystem.Instance.CallClient(characterOwner,
                                                 _ => _.ClientRemote_ItemBroke(robotItem.ProtoItem));
        Server.World.DestroyObject(objectRobot);
      }
    }

    private void ClientRemote_OnRobotDroppedOrReturned(
        ICharacter toCharacter,
        Vector2Ushort objectRobotTilePosition,
        bool isReturnedToPlayer)
    {
      if (toCharacter is not null
          && !toCharacter.IsInitialized)
      {
        toCharacter = null;
      }

      var time = Client.Core.ClientRealTime;
      if (time - this.lastRobotReturnSoundTime < 0.1)
      {
        //Logger.Dev("Robot return sound throttled");
        return;
      }

      this.lastRobotReturnSoundTime = time;
      //Logger.Dev("Robot return sound");

      if (toCharacter is not null)
      {
        Client.Audio.PlayOneShot(this.RobotReturnOrDropSoundResource, toCharacter);
      }
      else
      {
        Client.Audio.PlayOneShot(this.RobotReturnOrDropSoundResource,
                                 worldPosition: objectRobotTilePosition.ToVector2D() + (0.5, 0.5));
      }
    }

    private void ClientRemote_OnRobotStart(IDynamicWorldObject objectRobot)
    {
      if (!objectRobot.IsInitialized)
      {
        return;
      }

      var time = Client.Core.ClientRealTime;
      if (time - this.lastRobotStartSoundTime < 0.1)
      {
        //Logger.Dev("Robot start sound throttled");
        return;
      }

      this.lastRobotStartSoundTime = time;
      //Logger.Dev("Robot start sound at: " + objectRobot.Position);

      Client.Audio.PlayOneShot(this.RobotStartSoundResource, objectRobot);
    }

    private void ClientRemote_VehicleExploded(Vector2D position)
    {
      Logger.Important(this + " exploded at " + position);
      SharedExplosionHelper.ClientExplode(position: position + this.SharedGetObjectCenterWorldOffset(null),
                                          this.DestroyedExplosionPreset,
                                          this.DestroyedExplosionVolume);
    }

    private void ClientSetupHealthbar(ClientInitializeData data)
    {
      var objectRobot = data.GameObject;
      var publicState = data.PublicState;

      var structurePointsBarControl = new VehicleArmorBarControl();
      structurePointsBarControl.Setup(
          new ObjectStructurePointsData(objectRobot, this.SharedGetStructurePointsMax(objectRobot)),
          publicState.StructurePointsCurrent);

      Api.Client.UI.AttachControl(
          objectRobot,
          structurePointsBarControl,
          positionOffset: this.SharedGetObjectCenterWorldOffset(objectRobot) + (0, 0.55),
          isFocusable: false);
    }


  }

  public abstract class ProtoRobot<TItemRobot>
      : ProtoRobot
          <TItemRobot,
              RobotPrivateState,
              RobotPublicState,
              EmptyClientState>
      where TItemRobot : IProtoItemRobot, new()
  {
  }
}