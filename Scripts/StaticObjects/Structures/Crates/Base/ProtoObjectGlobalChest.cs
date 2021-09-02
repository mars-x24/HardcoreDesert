namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
  using AtomicTorch.CBND.CoreMod.ItemContainers;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
  using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Scripting;

  public abstract class ProtoObjectGlobalChest
    <TPrivateState,
     TPublicState,
     TClientState>
    : ProtoObjectCrate
      <TPrivateState,
          TPublicState,
          TClientState>,
      IInteractableProtoWorldObject
    where TPrivateState : ObjectCratePrivateState, new()
    where TPublicState : ObjectGlobalChestPublicState, new()
    where TClientState : StaticObjectClientState, new()
  {
    protected override IProtoItemsContainer ItemsContainerType =>  Api.GetProtoEntity<ItemsContainerGlobalStorage>();

   
    protected override void ServerInitialize(ServerInitializeData data)
    {
      base.ServerInitialize(data);

      var worldObject = data.GameObject;
      var privateState = data.PrivateState;
      if (data.IsFirstTimeInit)
      {
        privateState.DirectAccessMode = this.HasOwnersList
                                            ? WorldObjectDirectAccessMode.OpensToObjectOwnersOrAreaOwners
                                            : WorldObjectDirectAccessMode.OpensToEveryone;

        privateState.FactionAccessMode = WorldObjectFactionAccessModes.AllFactionMembers;
      }
      else if (!this.HasOwnersList)
      {
        privateState.DirectAccessMode = WorldObjectDirectAccessMode.OpensToEveryone;
      }

      WorldObjectOwnersSystem.ServerInitialize(worldObject);

      this.UpdateContainer(worldObject);
    }

    void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
    {
      var group = LandClaimGroup.GetGroup((IStaticWorldObject)worldObject);
      if (group is null)
      {
        return;
      }

      this.UpdateContainer((IStaticWorldObject)worldObject);

      var publicState = worldObject.GetPublicState<ObjectGlobalChestPublicState>();
      publicState.LandClaimGroup = group;
      Server.World.EnterPrivateScope(who, group);
    }

    void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
    {
      var group = LandClaimGroup.GetGroup((IStaticWorldObject)worldObject);
      if (group is null)
      {
        return;
      }


      Server.World.ExitPrivateScope(who, group);
    }

    protected override void ClientInteractStart(ClientObjectData data)
    {
      InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
    }

    private void UpdateContainer(IStaticWorldObject worldObject)
    {
      var landClaimGroup = LandClaimGroup.GetGroup(worldObject);
      if(landClaimGroup is null)
      {
        //old server
        var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObject);
        if (areasGroup is not null)
        {
          LandClaimGroup.ServerAreasGroupChanged(null, areasGroup);
          landClaimGroup = LandClaimGroup.GetGroup(worldObject);
        }
      }

      IItemsContainer itemsContainer = null;
      if (landClaimGroup is not null)
      {
        var landClaimGroupPrivateState = LandClaimGroup.GetPrivateState(landClaimGroup);
        itemsContainer = landClaimGroupPrivateState.ItemsContainerGlobalStorage;
      }

      var privateState = worldObject.GetPrivateState<ObjectCratePrivateState>();
      privateState.ItemsContainer = itemsContainer;
    }

  }

  public abstract class ProtoObjectGlobalChest
    : ProtoObjectGlobalChest<ObjectCratePrivateState, ObjectGlobalChestPublicState,
        StaticObjectClientState>
  {
  }
}