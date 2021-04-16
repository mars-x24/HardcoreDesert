namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
  using AtomicTorch.CBND.CoreMod.Characters.Mobs;
  using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
  using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
  using AtomicTorch.CBND.CoreMod.Systems.Weapons;
  using AtomicTorch.CBND.GameApi;
  using AtomicTorch.CBND.GameApi.Data.Weapons;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;
  using System;
  using System.Windows.Media;

  /// <summary>
  /// Desert Princess on death spectacularly explodes (akin to Pragmium Source)
  /// after a short delay creating remains.
  /// </summary>
  public class ObjectDesertPrincessDeathExplosion : ProtoObjectExplosive
  {
    public const int ExplosionDamageRadius = 13;

    public override double DamageRadius => ExplosionDamageRadius; // large radius

    public override TimeSpan ExplosionDelay => TimeSpan.FromSeconds(6);

    public override bool IsActivatesRaidBlock => false;

    public override bool IsDamageThroughObstacles => true;

    [NotLocalizable]
    public override string Name => "Desert Princess's death explosion";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

    public override float VolumeExplosion => 3;

    public override bool SharedOnDamage(
        WeaponFinalCache weaponCache,
        IStaticWorldObject targetObject,
        double damagePreMultiplier,
        out double obstacleBlockDamageCoef,
        out double damageApplied)
    {
      obstacleBlockDamageCoef = 0;
      damageApplied = 0; // no damage
      return true;       // hit
    }

    protected override ITextureResource ClientCreateIcon()
    {
      return Api.GetProtoEntity<MobBossPragmiumKing>().Icon;
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);
      var sceneObject = data.GameObject.ClientSceneObject;
      var componentBombCountdown = sceneObject.FindComponent<ClientComponentBombCountdown>();
      componentBombCountdown.IsRendering = false;
      this.ClientAddShakes(componentBombCountdown);

      var sceneObjectEffect = Client.Scene
                                    .CreateSceneObject("Desert Princess explosion effect",
                                                       position: sceneObject.Position);
      var soundEmitter = Api.Client.Audio.CreateSoundEmitter(
          sceneObjectEffect,
          soundResource: new SoundResource("Ambient/Earthquake"),
          is3D: true,
          worldPositionOffset: this.Layout.Center,
          radius: 20,
          isLooped: true);

      soundEmitter.Seek(0);
      soundEmitter.CustomMinDistance = ExplosionDamageRadius;
      soundEmitter.CustomMaxDistance = 10 + ExplosionDamageRadius;

      var lightSource = ObjectMineralPragmiumHelper.ClientInitializeLightForSource(sceneObjectEffect);
      lightSource.LogicalSize = lightSource.RenderingSize = 24;
      lightSource.Color = Color.FromArgb(100, 255, 100, 175);
      sceneObjectEffect
          .AddComponent<ClientComponentLightSourceEffectPulsating>()
          .Setup(data.GameObject,
                 safeDistance: ExplosionDamageRadius + 1, // add extra tile just to ensure safety
                 lightSource,
                 soundEmitter,
                 fromPercents: 85,
                 toPercents: 120,
                 durationSeconds: 1);
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      renderer.IsEnabled = false;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("#");
    }

    protected override void PrepareDamageDescriptionCharacters(
        out double damageValue,
        out double armorPiercingCoef,
        out double finalDamageMultiplier,
        DamageDistribution damageDistribution)
    {
      // nobody can survive this explosion
      damageValue = 10_000;
      armorPiercingCoef = 1;
      finalDamageMultiplier = 1;
      damageDistribution.Set(DamageType.Kinetic, 1);
    }

    protected override void PrepareDamageDescriptionStructures(
        out double damageValue,
        out double defencePenetrationCoef)
    {
      // nothing can survive this explosion
      damageValue = 10_000_000;
      defencePenetrationCoef = 1;
    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return TextureResource.NoTexture;
    }

    protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
    {
      explosionPresets = ExplosionPresets.SpecialPragmiumSourceExplosion;
    }

    protected override double SharedPrepareStatDamageToCharacters(double damageValue)
    {
      // we want to damage characters even if there is a reduced explosion damage rate (such as on a PvE server)
      return damageValue;
    }

    protected override void SharedProcessCreatedPhysics(CreatePhysicsData data)
    {
    }

    private void ClientAddShakes(ClientComponentBombCountdown component)
    {
      const float shakesInterval = 0.5f,
                  shakesDuration = 2f,
                  shakesDistanceMin = 0.2f,
                  shakesDistanceMax = 0.25f;

      if (component.IsDestroyed)
      {
        return;
      }

      var intensity = 1 - component.SecondsRemains / this.ExplosionDelay.TotalSeconds;
      if (intensity < 0.3)
      {
        intensity = 0.3;
      }

      ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                        worldDistanceMin: (float)(intensity * -shakesDistanceMin),
                                                        worldDistanceMax: (float)(intensity * shakesDistanceMax));

      ClientTimersSystem.AddAction(shakesInterval, () => this.ClientAddShakes(component));
    }
  }
}