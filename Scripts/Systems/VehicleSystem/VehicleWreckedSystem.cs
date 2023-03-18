using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.Rates;
using AtomicTorch.CBND.CoreMod.Systems;
using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
using AtomicTorch.CBND.CoreMod.Vehicles;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.Scripting.Network;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using System;

namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
  public class VehicleWreckedSystem : ProtoSystem<VehicleWreckedSystem>
  {
    public static void SpawnWreckedHoverboard(IStaticWorldObject gameObject)
    {
      byte chance = RateWreckedHoverboardChance.SharedValue;

      if (chance == 0)
        return;

      if (RandomHelper.Next(0, chance) != RandomHelper.Next(0, chance))
        return;

      var tilePosition = gameObject.TilePosition;

      ProtoVehicleHoverboard proto;
      if (RandomHelper.Next(0, 10) != RandomHelper.Next(0, 10))
        proto = Api.GetProtoEntity<VehicleWreckedHoverboardMk1>();
      else
        proto = Api.GetProtoEntity<VehicleWreckedHoverboardMk2>();

      var vehicle = Api.Server.World.CreateDynamicWorldObject(
                proto,
                position: tilePosition.ToVector2D());

      var privateState = vehicle.GetPrivateState<VehiclePrivateState>();
      if (ServerPlayerCharacterCurrentActionStateContext.CurrentCharacter is not null)
        privateState.Owners.Add(ServerPlayerCharacterCurrentActionStateContext.CurrentCharacter.Name);

      var result = Api.Server.Items.CreateItem<ItemFuelCellGasoline>(privateState.FuelItemsContainer);

      if (result.IsEverythingCreated)
      {
        foreach (var item in Api.Shared.WrapInTempList(privateState.FuelItemsContainer.Items).EnumerateAndDispose())
        {
          if (item.ProtoItem is IProtoItemWithDurability protoItemWithDurability)
          {
            // set the cell durability between 30% - 70%
            var privateStateCell = item.GetPrivateState<IItemWithDurabilityPrivateState>();
            var durability = protoItemWithDurability.DurabilityMax * RandomHelper.Next(30, 70) / 100.0;
            privateStateCell.DurabilityCurrent = (uint)Math.Round(durability, MidpointRounding.AwayFromZero);
          }
        }

        proto.ServerRefreshEnergyMax(vehicle);

        // notify other players in scope
        using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
        Api.Server.World.GetScopedByPlayers(vehicle, tempPlayers);

        Instance.CallClient(tempPlayers.AsList(),
                        _ => _.ClientRemote_OnVehicleBuiltByOtherPlayer(tilePosition.ToVector2D()));
      }
    }

    private void ClientRemote_OnVehicleBuiltByOtherPlayer(Vector2D position)
    {
      Api.Client.Audio.PlayOneShot(VehicleSystem.SoundResourceVehicleBuilt, position);
    }
  }
}