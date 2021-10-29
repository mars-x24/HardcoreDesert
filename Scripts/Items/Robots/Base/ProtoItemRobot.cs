namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
  using AtomicTorch.CBND.CoreMod.Characters;
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
  using AtomicTorch.GameEngine.Common.Primitives;
  using HardcoreDesert.Scripts.Robots.Base;
  using System;
  using System.Collections.Generic;

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

    public override double ServerUpdateIntervalSeconds => 10;

    protected override void ServerUpdate(ServerUpdateData data)
    {
      base.ServerUpdate(data);

      //look in which container the item is   
      if (data.GameObject.Container is null)
        return;

      var privateStateItem = data.GameObject.GetPrivateState<ItemRobotPrivateState>();
      var robotObject = privateStateItem.WorldObjectRobot;
      var robotProto = robotObject.ProtoGameObject as IProtoRobot;
      var publicStateRobot = robotObject.GetPublicState<RobotPublicState>();

      //low hp
      if (publicStateRobot.StructurePointsCurrent < 5.0f)
        return;

      var position = Vector2Ushort.Max;
      IWorldObject owner = null;
      var ownerContainer = data.GameObject.Container;
      IProtoEntity targetProto = null;

      if (data.GameObject.Container.OwnerAsStaticObject is not null)
      {
        if (data.GameObject.Container.OwnerAsStaticObject.ProtoGameObject is not ObjectGroundItemsContainer)  //nothing to do on ground       
        {
          owner = data.GameObject.Container.OwnerAsStaticObject;
          position = data.GameObject.Container.OwnerAsStaticObject.TilePosition;

          var publicState = owner.GetPublicState<ObjectCratePublicState>();
          if (publicState is not null)
            targetProto = publicState.IconSource;
        }
      }
      else if (data.GameObject.Container.OwnerAsCharacter is not null)
      {
        owner = data.GameObject.Container.OwnerAsCharacter;
        position = data.GameObject.Container.OwnerAsCharacter.TilePosition;

        var characterPrivateState = owner.GetPrivateState<PlayerCharacterPrivateState>();
        if (characterPrivateState.ContainerHand == data.GameObject.Container ||
            characterPrivateState.ContainerHotbar == data.GameObject.Container)
          owner = null; //must be in character inventory
      }

      if (owner is null)
        return;

      var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(position);
      if (areasGroup is null)
        return;

      IStaticWorldObject target = null;
      List<IItem> targetItems = new List<IItem>();

      int targetCount = 0;

      using (var temp = Api.Shared.GetTempList<IStaticWorldObject>())
      {
        foreach (var area in areasGroup.GetPrivateState<LandClaimAreasGroupPrivateState>().ServerLandClaimsAreas)
        {
          if (owner is ICharacter)
            if (!LandClaimSystem.ServerIsOwnedArea(area, (ICharacter)owner, false))
              continue;

          var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, false);
          temp.AddRange(Api.Server.World.GetStaticWorldObjectsOfProto<IProtoObjectManufacturer>());
        }

        foreach (IStaticWorldObject m in temp.AsList())
        {
          using (var items = Api.Shared.GetTempList<IItem>())
          {
            var itemOrdered = RobotTargetHelper.GetOutputContainersItems(m);

            List<IItem> tempTargetItems = new List<IItem>();
            int tempTargetCount = 0;

            foreach (var item in itemOrdered)
            {
              if (tempTargetItems.Count >= robotProto.ItemDeliveryCount)
                break;

              if (!RobotTargetHelper.ServerPickupAllowed(item, robotObject))
                continue;

              if (targetProto is not null && (item.ProtoGameObject.GetType() != targetProto.GetType()))
                continue;

              tempTargetItems.Add(item);
              tempTargetCount += item.Count;
            }

            if (tempTargetCount > targetCount && data.GameObject.Container.EmptySlotsCount >= tempTargetItems.Count)
            {
              target = m;
              targetItems.Clear();
              targetItems.AddRange(tempTargetItems);
              targetCount = tempTargetCount;
            }
            tempTargetItems.Clear();
          }
        }
      }

      //launch robot to target
      if (target is null)
        return;

      robotProto.ServerSetupAssociatedItem(robotObject, data.GameObject);

      robotProto.ServerStartRobot(robotObject, owner, ownerContainer);

      publicStateRobot.SetTargetPosition(target, targetItems);

      RobotTargetHelper.ServerTryRegisterCurrentPickup(targetItems, robotObject);
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

    protected override void PrepareHints(List<string> hints)
    {
      base.PrepareHints(hints);
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