﻿using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Systems.Droplists;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
  public interface IProtoItemFish : IProtoItemUsableFromContainer
  {
    FishingBaitWeightReadOnlyList BaitWeightList { get; }

    IReadOnlyDropItemsList DropItemsList { get; }

    bool IsSaltwaterFish { get; }

    bool IsLavaFish { get; }

    float MaxLength { get; }

    float MaxWeight { get; }

    byte RequiredFishingKnowledgeLevel { get; }

    /// <summary>
    /// Use this method to define additional condition when this fish could be caught.
    /// </summary>
    bool ServerCanCatch(ICharacter character, Vector2Ushort fishingTilePosition);
  }
}