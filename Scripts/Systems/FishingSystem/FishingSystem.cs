﻿using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
using AtomicTorch.CBND.CoreMod.Helpers.Client;
using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
using AtomicTorch.CBND.CoreMod.Items.Tools;
using AtomicTorch.CBND.CoreMod.Skills;
using AtomicTorch.CBND.CoreMod.Stats;
using AtomicTorch.CBND.CoreMod.Systems.Droplists;
using AtomicTorch.CBND.CoreMod.Systems.Notifications;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Tiles;
using AtomicTorch.CBND.CoreMod.UI;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.Logic;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using AtomicTorch.CBND.GameApi.ServicesServer;
using AtomicTorch.GameEngine.Common.DataStructures;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
  public class FishingSystem
        : ProtoActionSystem
            <FishingSystem,
                FishingActionRequest,
                FishingActionState,
                FishingActionPublicActionState>
  {
    public const double MaxFishingDistance = 10; //MOD

    public const string Notification_CannotFishHere = "Cannot fish here";

    // The random draw was unlucky so the fish was not caught.
    public const string Notification_FishSlipOffTheHook = "Fish managed to slip off the hook!";

    // The player didn't react in time.
    public const string Notification_TooLate = "Too late...you let the fish get away";

    private static readonly Dictionary<IProtoItemFish, IReadOnlyDropItemsList> ServerCachedDroplists
        = new();

    public static event DelegateServerFishCaught ServerFishCaught;

    public static IReadOnlyList<IProtoItemFish> AllFishList { get; private set; }

    public static void ClientPullFish()
    {
      Instance.CallServer(_ => _.ServerRemote_PullFish());
    }

    public static void ServerSendNotificationFishSlipOfTheHook(ICharacter character)
    {
      Instance.CallClient(character,
                          _ => _.ClientRemote_OnFishSlipOfTheHook());
    }

    public static void ServerSendNotificationTooLate(ICharacter character)
    {
      Instance.CallClient(character,
                          _ => _.ClientRemote_OnTooLate());
    }

    public static bool ServerTryDeductBait(ICharacter character, IProtoItemFishingBait protoItemBait)
    {
      var itemBait = SharedFindBaitItem(character, protoItemBait);
      if (itemBait is null)
      {
        return false;
      }

      // found a bait item of the required type, deduct the amount
      Server.Items.SetCount(itemBait,
                            itemBait.Count - 1,
                            byCharacter: character);

      NotificationSystem.ServerSendItemsNotification(character,
                                                     protoItemBait,
                                                     deltaCount: -1);

      return true;
    }

    public static IItem SharedFindBaitItem(ICharacter character, IProtoItemFishingBait currentProtoBait)
    {
      var lowestFreshness = uint.MaxValue;
      IItem lowestFreshnessItem = null;

      foreach (var container in character.ProtoCharacter.SharedEnumerateAllContainers(
          character,
          includeEquipmentContainer: false))
      {
        foreach (var itemBait in container.GetItemsOfProto(currentProtoBait))
        {
          var freshnessCurrent = itemBait.GetPrivateState<IItemWithFreshnessPrivateState>()
                                         .FreshnessCurrent;

          if (lowestFreshness <= freshnessCurrent)
          {
            continue;
          }

          lowestFreshness = freshnessCurrent;
          lowestFreshnessItem = itemBait;
        }
      }

      return lowestFreshnessItem;
    }

    public static bool SharedIsTooFar(ICharacter character, Vector2D fishingTargetPosition)
    {
      return character.Position.DistanceSquaredTo(fishingTargetPosition)
             > MaxFishingDistance * MaxFishingDistance;
    }

    protected override FishingActionRequest ClientTryCreateRequest(ICharacter character)
    {
      if (ComponentFishingVisualizer.TryGetFor(character, out _)
          || PlayerCharacter.GetPrivateState(character).CurrentActionState is FishingActionState)
      {
        // already fishing
        return null;
      }

      var itemFishingRod = character.SharedGetPlayerSelectedHotbarItem();
      var fishingRodPublicState = itemFishingRod.GetPublicState<ItemFishingRodPublicState>();

      if (fishingRodPublicState.CurrentProtoBait is null
          || SharedFindBaitItem(character, fishingRodPublicState.CurrentProtoBait) is null)
      {
        // no bait selected - try switch it
        FishingBaitReloadingSystem.FishingBaitReloadingSystem.ClientTrySwitchBaitType();
        if (fishingRodPublicState.CurrentProtoBait is null
            || SharedFindBaitItem(character, fishingRodPublicState.CurrentProtoBait) is null)
        {
          // no bait selected after a switch attempt
          return null;
        }
      }

      var fishingTargetPosition = ClientInputManager.MouseWorldPosition;

      // clamp max distance from player
      fishingTargetPosition = character.Position
                              + ((fishingTargetPosition - character.Position)
                                    .ClampMagnitude(MaxFishingDistance * 0.95));

      // validation will be done in SharedValidateRequest automatically
      return new FishingActionRequest(character, itemFishingRod, fishingTargetPosition);
    }

    protected override void PrepareSystem()
    {
      base.PrepareSystem();

      AllFishList = Api.FindProtoEntities<IProtoItemFish>().ToArray();
    }

    protected override void SharedOnActionCompletedInternal(FishingActionState state, ICharacter character)
    {
      // please note: a successful catch is handled by ServerRemote_PullFish method
    }

    protected override FishingActionState SharedTryCreateState(FishingActionRequest request)
    {
      var state = new FishingActionState(request.Character,
                                         FishingSession.MaxDuration,
                                         request.Item,
                                         request.FishingTargetPosition);

      if (IsClient)
      {
        return state;
      }

      // send fishing session to the client
      Api.Server.World.ForceEnterScope(request.Character, state.SharedFishingSession);
      this.CallClient(request.Character,
                      _ => _.ClientRemote_OnFishingSessionStarted(request, state.SharedFishingSession));

      return state;
    }

    protected override void SharedValidateRequest(FishingActionRequest request)
    {
      var character = request.Character;
      var fishingTargetPosition = request.FishingTargetPosition;
      var itemRod = request.Item;

      if (itemRod is null
          || itemRod != character.SharedGetPlayerSelectedHotbarItem()
          || itemRod.ProtoItem is not IProtoItemToolFishing)
      {
        throw new Exception("The fishing rod is not selected");
      }

      var fishingRodPublicState = itemRod.GetPublicState<ItemFishingRodPublicState>();
      if (fishingRodPublicState.CurrentProtoBait is null
          || SharedFindBaitItem(character, fishingRodPublicState.CurrentProtoBait) is null)
      {
        // no bait available
        throw new Exception("There is no bait available of the required type");
      }

      var world = IsServer
                      ? (IWorldService)Server.World
                      : (IWorldService)Client.World;
      var tile = world.GetTile(fishingTargetPosition.ToVector2Ushort());
      if (tile.ProtoTile is not IProtoTileWater protoTileWater
          || !protoTileWater.IsFishingAllowed)
      {
        if (IsClient)
        {
          CannotInteractMessageDisplay.ClientOnCannotInteract(character,
                                                              Notification_CannotFishHere,
                                                              isOutOfRange: false);
        }

        throw new Exception("Cannot fish here");
      }

      // check obstacle objects (such as bridges)
      using var testResults = world.GetPhysicsSpace()
                                   .TestPoint(fishingTargetPosition,
                                              CollisionGroups.Default,
                                              sendDebugEvent: false);

      foreach (var testResult in testResults.AsList())
      {
        if (testResult.PhysicsBody.AssociatedWorldObject is null)
        {
          continue;
        }

        // an obstacle found
        if (IsClient)
        {
          CannotInteractMessageDisplay.ClientOnCannotInteract(character,
                                                              Notification_CannotFishHere,
                                                              isOutOfRange: false);
        }

        throw new Exception("Cannot fish here");
      }

      if (SharedIsTooFar(character, fishingTargetPosition))
      {
        if (IsClient)
        {
          CannotInteractMessageDisplay.ClientOnCannotInteract(character,
                                                              CoreStrings.Notification_TooFar,
                                                              isOutOfRange: true);
        }

        throw new Exception("Too far");
      }
    }

    private static IReadOnlyDropItemsList ServerGetDroplistForFish(IProtoItemFish protoItemFish)
    {
      if (ServerCachedDroplists.TryGetValue(protoItemFish, out var droplist))
      {
        return droplist;
      }

      droplist = new DropItemsList().Add(protoItemFish);
      ServerCachedDroplists[protoItemFish] = droplist;
      return droplist;
    }

    private static ArrayWithWeights<IProtoItemFish> ServerSelectAvailableFishPrototypes(
        ICharacter character,
        bool isSaltWater, bool isLava,
        Vector2Ushort fishingTilePosition,
        IProtoItemFishingBait protoItemFishingBait,
        byte characterFishingKnowledgeLevel)
    {
      using var tempList = Api.Shared.GetTempList<ValueWithWeight<IProtoItemFish>>();

      foreach (var protoItemFish in AllFishList)
      {
        if (protoItemFish.IsSaltwaterFish != isSaltWater || protoItemFish.IsLavaFish != isLava)
        {
          // this fish is for different water type
          continue;
        }

        if (!protoItemFish.ServerCanCatch(character, fishingTilePosition))
        {
          continue;
        }

        if (protoItemFish.RequiredFishingKnowledgeLevel > characterFishingKnowledgeLevel)
        {
          // cannot catch this fish yet
          continue;
        }

        var weight = protoItemFish.BaitWeightList.GetWeightForBait(protoItemFishingBait);
        if (weight <= 0)
        {
          // this fish is not interested in this bait
          continue;
        }

        tempList.Add(new ValueWithWeight<IProtoItemFish>(protoItemFish, weight));
      }

      return new ArrayWithWeights<IProtoItemFish>(tempList.AsList());
    }

    private void ClientRemote_OnFishCaught(
        ICharacter character,
        IProtoItem protoItemFish,
        Vector2D fishingTargetPosition,
        float sizeValue)
    {
      ComponentFishingCompletedVisualizer.OnFishCaughtOrFishingCancelled(
          character,
          protoItemFishCaught: protoItemFish,
          fishingTargetPosition,
          caughtFishSizeValue: sizeValue);
    }

    private void ClientRemote_OnFishingSessionStarted(
        FishingActionRequest request,
        ILogicObject currentFishingSession)
    {
      // execute next frame as currentFishingSession is not initialized yet
      ClientTimersSystem.AddAction(
          0,
          () =>
          {
            if (ClientCurrentCharacterHelper.PrivateState.CurrentActionState
                          is not FishingActionState actionState
                      || !actionState.Request.Equals(request))
            {
              return;
            }

            //Logger.Dev("Client received fishing session: " + currentFishingSession);
            actionState.ClientSetCurrentFishingSession(currentFishingSession);
          });
    }

    private void ClientRemote_OnFishSlipOfTheHook()
    {
      CannotInteractMessageDisplay.ClientOnCannotInteract(ClientCurrentCharacterHelper.Character,
                                                          Notification_FishSlipOffTheHook,
                                                          isOutOfRange: false,
                                                          hideDelay: 3,
                                                          playSound: false);

      ClientFishingSoundsHelper.PlaySoundFail(ClientCurrentCharacterHelper.Character);
    }

    private void ClientRemote_OnTooLate()
    {
      CannotInteractMessageDisplay.ClientOnCannotInteract(ClientCurrentCharacterHelper.Character,
                                                          Notification_TooLate,
                                                          isOutOfRange: false,
                                                          hideDelay: 3,
                                                          playSound: false);

      ClientFishingSoundsHelper.PlaySoundFail(ClientCurrentCharacterHelper.Character);
    }

    [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
    private void ServerRemote_PullFish()
    {
      var character = ServerRemoteContext.Character;
      if (PlayerCharacter.GetPrivateState(character).CurrentActionState
              is not FishingActionState state)
      {
        // received too late, the fishing session has ended
        return;
      }

      var fishingSession = state.SharedFishingSession;
      var isBiting = FishingSession.GetPublicState(fishingSession).IsFishBiting;
      //Logger.Dev("Client requested pulling the fish. IsBiting=" + isBiting, character);

      try
      {
        if (!isBiting)
        {
          // no fish to pull
          return;
        }

        byte baitCount = state.ServerTryToDeductTheBait();

        if (baitCount == 0)
        {
          // don't have the bait to deduct (perhaps player dropped it or it's spoiled after fishing started)
          return;
        }

        var rodPublicState = state.ItemFishingRod.GetPublicState<ItemFishingRodPublicState>();
        var fishingTilePosition = state.Request.FishingTargetPosition.ToVector2Ushort();
        var protoTile = Server.World.GetTile(fishingTilePosition).ProtoTile;
        var isSaltWater = protoTile is TileWaterSea;
        var isLava = protoTile is TileLava;

        for (int i = 0; i < baitCount; i++)
        {
          state.ServerIsPlayerTriedToCatch = true;

          var fishCatchChance = character.SharedGetFinalStatMultiplier(StatName.FishingSuccess) - 1;

          if (isLava)
            fishCatchChance /= 10;

          fishCatchChance = MathHelper.Clamp(fishCatchChance, 0, 1);

          if (!RandomHelper.RollWithProbability(fishCatchChance))
          {
            // a bad random draw, fish got away
            continue;
          }

          var fishingKnowledgeCoef = character.SharedGetFinalStatValue(StatName.FishingKnowledgeLevel)
                                     / 100.0;

          // bridges are increasing the fishing knowledge level
          fishingKnowledgeCoef += ((ProtoTile)character.Tile.ProtoTile).FishingKnowledgeLevelIncrease
                                  / 100.0;

          fishingKnowledgeCoef = MathHelper.Clamp(fishingKnowledgeCoef, 0, 1);

          state.ServerDeductDurability();

          var selectedFishPrototypes = ServerSelectAvailableFishPrototypes(
              character,
              isSaltWater,
              isLava,
              fishingTilePosition,
              rodPublicState.CurrentProtoBait,
              characterFishingKnowledgeLevel: (byte)(100 * fishingKnowledgeCoef));

          if (selectedFishPrototypes.Count == 0)
            continue;

          var protoItemFish = selectedFishPrototypes.GetSingleRandomElement();
          var droplist = ServerGetDroplistForFish(protoItemFish);
          var createItemResult = droplist.TryDropToCharacterOrGround(
              character,
              character.TilePosition,
              new DropItemContext(character),
              out _,
              // no rate is applied here, but the basic gathering rate
              // is applied in ProtoItemFish.ServerRemote_Cut method
              probabilityMultiplier: 1.0);

          var fishCaught = createItemResult.ItemAmounts.FirstOrDefault().Key;
          if (fishCaught is null)
          {
            // should not be possible
            continue;
          }

          state.ServerIsSuccess = true;

          NotificationSystem.ServerSendItemsNotification(character, createItemResult);

          // size formula: 0.15 + 0.1 * random + 0.75 * skillCoef * random
          var sizeValue = (float)(0.15
                                  + 0.1 * RandomHelper.NextDouble()
                                  + 0.75 * fishingKnowledgeCoef * RandomHelper.NextDouble());

          character.ServerAddSkillExperience<SkillFishing>(SkillFishing.ExperienceForCaughtFish);

          Logger.Info("Fishing success: " + fishCaught, character);

          Api.SafeInvoke(() => ServerFishCaught?.Invoke(character, fishCaught, sizeValue));

          using var tempObservers = Api.Shared.GetTempList<ICharacter>();
          Server.World.GetScopedByPlayers(character, tempObservers);
          tempObservers.Add(character);
          this.CallClient(tempObservers.AsList(),
                          _ => _.ClientRemote_OnFishCaught(character,
                                                           fishCaught.ProtoItem,
                                                           state.FishingTargetPosition,
                                                           sizeValue));
        }
      }
      finally
      {
        state.ServerSetCompleted();
      }

    }
  }
}