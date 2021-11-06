namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
  using AtomicTorch.CBND.CoreMod.ItemContainers;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Logic;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Scripting;
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;

  [PrepareOrder(typeof(LandClaimArea))]
  public class LandClaimGroup
        : ProtoGameObject<ILogicObject,
              LandClaimGroupPrivateState,
              LandClaimGroupPublicState,
              EmptyClientState>,
          IProtoLogicObjectWithInteraction
  {
    public override double ClientUpdateIntervalSeconds => double.MaxValue; // never

    [NotLocalizable]
    public override string Name => "Land claim group";

    public override double ServerUpdateIntervalSeconds => double.MaxValue; // never

    public bool SharedCanInteract(ICharacter character, ILogicObject logicObject, bool writeToLog)
    {
      if (IsClient)
      {
        // we cannot perform this check on the client side yet
        return true;
      }

      return Server.World.IsInPrivateScope(logicObject, character);
    }

    protected override void ServerInitialize(ServerInitializeData data)
    {
      var itemsContainerGlobalStorage = data.PrivateState.ItemsContainerGlobalStorage;
      var globalStorageCapacity = ItemsContainerGlobalStorage.ServerGlobalStorageItemsSlotsCapacity;

      if (itemsContainerGlobalStorage is null)
      {
        itemsContainerGlobalStorage = Server.Items.CreateContainer<ItemsContainerGlobalStorage>(
            owner: data.GameObject,
            slotsCount: globalStorageCapacity);

        data.PrivateState.ItemsContainerGlobalStorage = itemsContainerGlobalStorage;
      }
      else
      {
        // container already created - update slots count
        Server.Items.SetSlotsCount(itemsContainerGlobalStorage,
                                   slotsCount: Math.Max(itemsContainerGlobalStorage.SlotsCount,
                                                        globalStorageCapacity));
      }
    }

    public static void ServerAreasGroupChanged(ILogicObject areasGroupFrom, ILogicObject newAreaGroups)
    {
      ServerAreasGroupChanged(areasGroupFrom, new List<ILogicObject>() { newAreaGroups });
    }

    public static void ServerAreasGroupChanged(ILogicObject areasGroupFrom, List<ILogicObject> newAreaGroups)
    {
      using var tempList = Api.Shared.GetTempList<ILogicObject>();
      Api.GetProtoEntity<LandClaimGroup>()
         .GetAllGameObjects(tempList.AsList());

      if (areasGroupFrom is not null)
      {
        foreach (var group in tempList.AsList())
        {
          var areaGroups = LandClaimGroup.GetPrivateState(group).ServerLandClaimAreasGroups;
          if (areaGroups is null)
            continue;

          if(areaGroups.Contains(areasGroupFrom))
          {
            areaGroups.Remove(areasGroupFrom);
            foreach (var newAreaGroup in newAreaGroups)
            {
              if (newAreaGroup is not null)
                areaGroups.Add(newAreaGroup);
            }
            //if (areaGroups.Count == 0)
            //  Api.Server.World.DestroyObject(group);
            return;
          }
        }
      }

      if (newAreaGroups[0] is null)
        return;

      ILogicObject groupFound = null;

      //search for owners
      string factionClanTag = "";
      var newAreaGroupPublicState = LandClaimAreasGroup.GetPublicState(newAreaGroups[0]);
      factionClanTag = newAreaGroupPublicState.FactionClanTag;

      var newAreaGroupPrivateState = LandClaimAreasGroup.GetPrivateState(newAreaGroups[0]);
      List<string> owners = new List<string>();
      GetAreaGroupOwners(owners, newAreaGroupPrivateState);

      foreach (var group in tempList.AsList())
      {
        var groupPrivateState = LandClaimGroup.GetPrivateState(group);
        if (groupPrivateState is null || groupPrivateState.ServerLandClaimAreasGroups is null)
          continue;

        if(!string.IsNullOrEmpty(factionClanTag) && groupPrivateState.FactionClanTag == factionClanTag)
        {
          groupFound = group;
          break;
        }

        if(groupPrivateState.owners is not null && groupPrivateState.owners.Intersect(owners).Any())
        {
          groupFound = group;
          break;
        }

        var areaGroupsTest = groupPrivateState.ServerLandClaimAreasGroups;
        if (areaGroupsTest.Count > 0)
        {
          foreach (var areaGroup in areaGroupsTest)
          {
            if (areaGroup is null)
              continue;

            if (LandClaimAreasGroup.GetPublicState(areaGroup).FactionClanTag == factionClanTag)
            {
              groupFound = group;
              break;
            }
          }
        }

        List<string> ownersAreasGroup = new List<string>();
        GetAreaGroupOwners(ownersAreasGroup, groupPrivateState);
        if (ownersAreasGroup.Intersect(owners).Any())
        {
          groupFound = group;
          break;
        }
      }

      if (groupFound is null)
      {
        groupFound = Api.Server.World.CreateLogicObject<LandClaimGroup>();  
      }

      var groupFoundPrivateState = LandClaimGroup.GetPrivateState(groupFound);
      if (groupFoundPrivateState.ServerLandClaimAreasGroups is null)
        groupFoundPrivateState.ServerLandClaimAreasGroups = new List<ILogicObject>();

      groupFoundPrivateState.ServerLandClaimAreasGroups.AddRange(newAreaGroups);

      groupFoundPrivateState.FactionClanTag = factionClanTag;

      groupFoundPrivateState.owners = owners;
    }

    private static void GetAreaGroupOwners(List<string> owners, LandClaimGroupPrivateState groupPrivateState)
    {
      if (groupPrivateState.ServerLandClaimAreasGroups.Count > 0)
      {
        foreach (var areasGroup in groupPrivateState.ServerLandClaimAreasGroups)
        {
          if (areasGroup is null)
            continue;

          var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
          GetAreaGroupOwners(owners, areasGroupPrivateState);
        }
      }
    }

    private static void GetAreaGroupOwners(List<string> owners, LandClaimAreasGroupPrivateState areasGroupPrivateState)
    {
      foreach (var area in areasGroupPrivateState.ServerLandClaimsAreas)
      {
        var areaPrivateState = area.GetPrivateState<LandClaimAreaPrivateState>();
        foreach (string owner in areaPrivateState.ServerGetLandOwners())
        {
          if (!owners.Contains(owner))
            owners.Add(owner);
        }
      }
    }

    public static ILogicObject GetGroup(IStaticWorldObject worldObject)
    {
      ILogicObject areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObject);
      if (areasGroup is null)
        return null;

      using var tempList = Api.Shared.GetTempList<ILogicObject>();
      Api.GetProtoEntity<LandClaimGroup>()
         .GetAllGameObjects(tempList.AsList());

      foreach (var group in tempList.AsList())
      {
        var areaGroupsTest = LandClaimGroup.GetPrivateState(group).ServerLandClaimAreasGroups;
        if (areaGroupsTest.Contains(areasGroup))
          return group;
      }

      return null;
    }

    public class BootstrapperLandClaimGroup : BaseBootstrapper
    {
      public override void ServerInitialize(IServerConfiguration serverConfiguration)
      {
        base.ServerInitialize(serverConfiguration);
        LandClaimSystem.ServerAreasGroupChanged += LandClaimSystem_ServerAreasGroupChanged;
      }

      private void LandClaimSystem_ServerAreasGroupChanged(ILogicObject area, ILogicObject areasGroupFrom, ILogicObject areasGroupTo)
      {
        LandClaimGroup.ServerAreasGroupChanged(areasGroupFrom, areasGroupTo);
      }
    }
  }
}