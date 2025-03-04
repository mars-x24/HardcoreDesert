﻿using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
using AtomicTorch.CBND.GameApi.Data.State;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
  public abstract class ProtoObjectTradingStationFridgeElectrical
      <TPrivateState,
       TPublicState,
       TClientState>
      : ProtoObjectTradingStationFridge
        <TPrivateState,
            TPublicState,
            TClientState>,
        IProtoObjectElectricityConsumer
      where TPrivateState : ObjectTradingStationFridgePrivateState, new()
      where TPublicState : ObjectTradingStationFridgePublicState, new()
      where TClientState : ObjectTradingStationClientState, new()
  {
    public virtual ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
        => new(startupPercent: 1,
               shutdownPercent: 0);

    public abstract double ElectricityConsumptionPerSecondWhenActive { get; }

    public override double ServerGetCurrentFreshnessDurationMultiplier(IStaticWorldObject worldObject)
    {
      var publicState = GetPublicState(worldObject);
      if (publicState.ElectricityConsumerState
          != ElectricityConsumerState.PowerOnActive)
      {
        // no power supplied so no freshness increase
        return 1;
      }

      return this.FreshnessDurationMultiplier;
    }

    IObjectElectricityStructurePrivateState IProtoObjectElectricityConsumer.GetPrivateState(
        IStaticWorldObject worldObject)
    {
      return GetPrivateState(worldObject);
    }

    IObjectElectricityConsumerPublicState IProtoObjectElectricityConsumer.GetPublicState(
        IStaticWorldObject worldObject)
    {
      return GetPublicState(worldObject);
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);

      var soundEmitter = Client.Audio.CreateSoundEmitter(
          data.GameObject,
          new SoundResource("Objects/Structures/ObjectFridge/Active"),
          isLooped: true,
          volume: 0.35f,
          radius: 1f);
      soundEmitter.CustomMaxDistance = 3.5f;
      this.ClientSetupSoundEmitter(soundEmitter);

      var publicState = data.PublicState;
      publicState.ClientSubscribe(_ => _.ElectricityConsumerState,
                                  _ => RefreshSoundEmitterState(),
                                  data.ClientState);

      RefreshSoundEmitterState();

      void RefreshSoundEmitterState()
      {
        soundEmitter.IsEnabled = publicState.ElectricityConsumerState
                                 == ElectricityConsumerState.PowerOnActive;
      }
    }

    protected virtual void ClientSetupSoundEmitter(IComponentSoundEmitter soundEmitter)
    {
    }
  }

  public abstract class ProtoObjectTradingStationFridgeElectrical
      : ProtoObjectTradingStationFridgeElectrical<
          ObjectTradingStationFridgePrivateState,
          ObjectTradingStationFridgePublicState,
          ObjectTradingStationClientState>
  {
  }
}