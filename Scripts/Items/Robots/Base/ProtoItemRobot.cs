namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.Characters;
  using AtomicTorch.CBND.CoreMod.Characters.Player;
  using AtomicTorch.CBND.CoreMod.Robots;
  using AtomicTorch.CBND.CoreMod.StaticObjects;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
  using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.Network;
  using AtomicTorch.GameEngine.Common.Primitives;
  using HardcoreDesert.Scripts.Robots.Base;
  using HardcoreDesert.UI.Controls.Game.WorldObjects.Robot;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public abstract class ProtoItemRobot
        <TObjectRobot,
         TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWithDurability
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemRobot
        where TObjectRobot : IProtoRobot, new()
        where TPrivateState : ItemRobotPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
  {
    private static readonly Lazy<TObjectRobot> LazyProtoRobot
        = new(Api.GetProtoEntity<TObjectRobot>);

    private readonly Lazy<double> lazyDurabilityToStructurePointsConversionCoefficient;

    protected ProtoItemRobot()
    {
      this.lazyDurabilityToStructurePointsConversionCoefficient = new Lazy<double>(
          () => this.ProtoRobot.StructurePointsMax / this.DurabilityMax);
    }

    public double DurabilityToStructurePointsConversionCoefficient
        => this.lazyDurabilityToStructurePointsConversionCoefficient.Value;

    public override double GroundIconScale => 1.6;

    public override bool IsRepairable => true;

    public IProtoRobot ProtoRobot => LazyProtoRobot.Value;

    public override double ServerUpdateIntervalSeconds => 1;

    public string ItemUseCaption => "";

    public virtual byte ItemDeliveryCount => 1;

    public virtual ushort DeliveryTimerSeconds => 10;

    protected override void ServerUpdate(ServerUpdateData data)
    {
      base.ServerUpdate(data);

      //look in which container the item is   
      if (data.GameObject.Container is null)
        return;

      var privateStateItem = data.GameObject.GetPrivateState<ItemRobotPrivateState>();
      var robotObject = privateStateItem.WorldObjectRobot;

      var privateStateRobot = robotObject.GetPrivateState<RobotPrivateState>();

      //wait for timer
      privateStateRobot.TimerInactive += data.DeltaTime;

      if (data.PrivateState.TimeRunIntervalSeconds == 0)
        return;

      ushort timeBetweenRuns = Math.Max(data.PrivateState.TimeRunIntervalSeconds, this.DeliveryTimerSeconds);

      if (privateStateRobot.TimerInactive < timeBetweenRuns)
        return;

      var robotProto = robotObject.ProtoGameObject as IProtoRobot;
      var publicStateRobot = robotObject.GetPublicState<RobotPublicState>();

      //low hp
      if (publicStateRobot.StructurePointsCurrent < 5.0f)
        return;

      var position = Vector2Ushort.Max;
      IWorldObject owner = null;
      var ownerContainer = data.GameObject.Container;
      IProtoEntity targetItemProto = null;

      if (ownerContainer is null)
        return;

      if (ownerContainer.OwnerAsStaticObject is not null)
      {
        if (ownerContainer.OwnerAsStaticObject.ProtoGameObject is not ObjectGroundItemsContainer)  //nothing to do on ground       
        {
          owner = ownerContainer.OwnerAsStaticObject;
          position = ownerContainer.OwnerAsStaticObject.TilePosition;

          var publicState = owner.GetPublicState<ObjectCratePublicState>();
          if (publicState is not null)
            targetItemProto = publicState.IconSource;
        }
      }
      else if (ownerContainer.OwnerAsCharacter is not null)
      {
        owner = ownerContainer.OwnerAsCharacter;
        position = ownerContainer.OwnerAsCharacter.TilePosition;

        var characterPrivateState = owner.GetPrivateState<PlayerCharacterPrivateState>();
        if (characterPrivateState.ContainerHand == ownerContainer ||
            characterPrivateState.ContainerHotbar == ownerContainer)
          owner = null; //must be in character inventory
      }
      else if (ownerContainer.Owner is LandClaimGroup)
      {
        //owner = ownerContainer.Owner;

      }

      if (owner is null)
        return;

      var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(position);
      if (areasGroup is null)
        return;

      var outputManufacturer = new List<IStaticWorldObject>();
      var inputManufacturer = new List<IStaticWorldObject>();

      foreach (var area in areasGroup.GetPrivateState<LandClaimAreasGroupPrivateState>().ServerLandClaimsAreas)
      {
        if (owner is ICharacter)
          if (!LandClaimSystem.ServerIsOwnedArea(area, (ICharacter)owner, false))
            continue;

        var areaPrivateState = area.GetPrivateState<LandClaimAreaPrivateState>();
        if (!areaPrivateState.RobotManufacturerInputEnabled && !areaPrivateState.RobotManufacturerOutputEnabled)
          continue;

        if (!areaPrivateState.RobotManufacturerCharacterInventoryEnabled && owner is ICharacter)
          continue;

        var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, false);

        using (var temp = Api.Shared.GetTempList<IStaticWorldObject>())
        {
          temp.AddRange(
            Api.Server.World.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectManufacturer>(bounds)
              .Distinct()
              .Where(m => RobotTargetHelper.ServerStructureAllowed(m, robotObject, privateStateItem)));

          if (areaPrivateState.RobotManufacturerOutputEnabled)
            outputManufacturer.AddRange(temp.AsList());

          if (areaPrivateState.RobotManufacturerInputEnabled)
            inputManufacturer.AddRange(temp.AsList());
        }
      }

      if (outputManufacturer.Count == 0 && inputManufacturer.Count == 0)
        return;

      var itemHelper = new RobotItemHelper(robotObject, robotProto, data.GameObject.Container, targetItemProto, outputManufacturer, inputManufacturer);

      itemHelper.FindAllItems();

      //launch robot to target
      if (itemHelper.Target is null)
        return;

      //nothing to do
      if (itemHelper.InputItems.Count + itemHelper.FuelItems.Count + itemHelper.TargetItems.Count == 0)
        return;

      if (!RobotTargetHelper.ServerStructureAllowed(itemHelper.Target, robotObject, privateStateItem))
        return;

      if (itemHelper.TargetItems.Count != 0 && !RobotTargetHelper.ServerPickupAllowed(itemHelper.TargetItems.Keys, robotObject))
        return;

      robotProto.ServerSetupAssociatedItem(robotObject, data.GameObject);

      robotProto.ServerStartRobot(robotObject, owner, ownerContainer);

      publicStateRobot.SetTargetPosition(itemHelper.Target, itemHelper.TargetItems, itemHelper.InputItems, itemHelper.FuelItems);

      if (!RobotTargetHelper.ServerTryRegisterCurrentPickup(publicStateRobot.TargetItems.Keys.ToList(), publicStateRobot.Target, robotObject, privateStateItem))
        publicStateRobot.ResetTargetPosition();
    }

    public override void ServerOnDestroy(IItem gameObject)
    {
      base.ServerOnDestroy(gameObject);

      var objectRobot = GetPrivateState(gameObject).WorldObjectRobot;
      if (objectRobot is not null
          && !objectRobot.IsDestroyed)
      {
        Server.World.DestroyObject(objectRobot);
      }
    }

    protected override void ClientItemUseStart(ClientItemData data)
    {
      base.ClientItemUseStart(data);

      this.CallServer(_ => _.ServerRemote_Use(data.Item));
    }

    protected override bool ClientItemUseFinish(ClientItemData data)
    {
      return true;
    }

    private void ServerRemote_Use(IItem item)
    {
      var character = ServerRemoteContext.Character;

      this.ServerValidateItemForRemoteCall(item, character);

      if (item.Container != PlayerCharacterItemsExtensions.SharedGetPlayerContainerInventory(character) &&
          item.Container != PlayerCharacterItemsExtensions.SharedGetPlayerContainerHotbar(character) &&
          item.Container != PlayerCharacterItemsExtensions.SharedGetPlayerContainerHotbar(character))
      {
        Api.Server.World.ForceEnterScope(character, item);
        Api.Server.World.EnterPrivateScope(character, item);
      }

      this.CallClient(character, _ => _.ClientRemote_OpenWindow(item));
    }

    private void ClientRemote_OpenWindow(IItem item)
    {
      WindowItemRobot.Open(item);
    }

    protected override void PrepareHints(List<string> hints)
    {
      base.PrepareHints(hints);
      hints.Add(ItemHints.AltClickToUseItem);
      hints.Add("Can hold " + this.ItemDeliveryCount + " stack" + (this.ItemDeliveryCount > 1 ? "s" : "") + " of items.");
      hints.Add("Minimum of " + this.DeliveryTimerSeconds + " seconds between runs.");
      hints.Add("You must allow robots inside your land claim building.");
    }

    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      data.GameObject.ProtoGameObject.ServerSetUpdateRate(data.GameObject, false);

      if (!data.IsFirstTimeInit)
      {
        return;
      }

      var itemRobot = data.GameObject;
      var protoRobot = LazyProtoRobot.Value;
      var objectRobot = Server.World.CreateDynamicWorldObject(
          protoRobot,
          CharacterDespawnSystem.ServerGetServiceAreaPosition().ToVector2D());
      protoRobot.ServerSetupAssociatedItem(objectRobot, itemRobot);
      data.PrivateState.WorldObjectRobot = objectRobot;
      data.PrivateState.StructureLoadPercent = ItemRobotPrivateState.DEFAULT_STRUCTURE_LOAD_PERCENT;
      data.PrivateState.TimeRunIntervalSeconds = this.DeliveryTimerSeconds;
    }
  }

  public abstract class ProtoItemRobot<TObjectRobot>
      : ProtoItemRobot
          <TObjectRobot,
              ItemRobotPrivateState,
              EmptyPublicState,
              EmptyClientState>
      where TObjectRobot : IProtoRobot, new()
  {
  }
}