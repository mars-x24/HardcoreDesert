using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.Robots;
using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using HardcoreDesert.Scripts.Systems.Robot;
using HardcoreDesert.UI.Controls.Game.WorldObjects.Robot;
using System;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.Items.Robots
{
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

      //clear the old structure list
      if (data.PrivateState.AllowedStructure?.Count > 0)
      {
        if (data.PrivateState.AllowedStructures is null)
          data.PrivateState.AllowedStructures = new List<StaticObjects.Structures.IProtoObjectStructure>();

        data.PrivateState.AllowedStructures.AddRange(data.PrivateState.AllowedStructure);
        data.PrivateState.AllowedStructure.Clear();
      }

      RobotSystem.TryStartRobotFromContainer(this, data.GameObject, data.PrivateState, data.DeltaTime);
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