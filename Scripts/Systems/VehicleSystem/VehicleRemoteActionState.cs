﻿using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;

namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
  public class VehicleRemoteActionState
        : BaseSystemActionState<
            VehicleRemoteSystem,
            ItemActionRequest,
            VehicleRemoteActionState,
            VehicleRemoteActionState.PublicState>
  {
    public override bool IsBlockingMovement => true;

    public override bool IsDisplayingProgress => true;

    public readonly IItem ItemVehicle;

    public VehicleRemoteActionState(
        ICharacter character,
        double durationSeconds,
        IItem itemVehicle)
        : base(character, null, durationSeconds)
    {
      this.ItemVehicle = itemVehicle;
    }

    public class PublicState : BasePublicActionState
    {
      protected override void ClientOnCompleted()
      {
        if (this.IsCancelled)
        {
          return;
        }
      }

      protected override void ClientOnStart()
      {

      }
    }
  }
}