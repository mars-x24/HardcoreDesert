using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
using AtomicTorch.CBND.CoreMod.Helpers;
using AtomicTorch.CBND.CoreMod.Objects;
using AtomicTorch.CBND.CoreMod.Stats;
using AtomicTorch.CBND.CoreMod.Tiles;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.ServicesServer;
using AtomicTorch.GameEngine.Common.Helpers;
using AtomicTorch.GameEngine.Common.Primitives;
using System;
using System.Linq;

namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
  public class StatusEffectColdSource : ProtoRadiantStatusEffect
  {
    public const double DamagePerSecondByIntensity = 10;

    private const int EnvironmentalTileColdLookupAreaDiameter = 9;

    private static readonly IWorldServerService ServerWorld = IsServer ? Server.World : null;

    private ProtoTile serverProtoTileIce;
    private ProtoTile serverProtoTileDarkIce;
    private ProtoTile serverProtoTileSnow;
    private ProtoTile serverProtoTileWaterSea;

    private Vector2Int[] serverTileOffsetsCircle;

    public override string Description =>
        "You are exposed to a high level of cold from a nearby cold source. Immediately leave the area to prevent further damage.";

    public override StatusEffectKind Kind => StatusEffectKind.Debuff;

    public override string Name => "Cold source";

    public override double ServerUpdateIntervalSeconds => 0.1;

    protected override StatName DefenseStatName => StatName.DefenseCold;

    /// <summary>
    /// Time to remove full effect intensity back to zero in case the environmental intensity is 0.
    /// </summary>
    protected override double TimeToCoolDownToZeroSeconds => 3;

    /// <summary>
    /// Time to reach the full intensity in case the environmental intensity is 1.
    /// </summary>
    protected override double TimeToReachFullIntensitySeconds => 4;

    protected override void ClientDeinitialize(StatusEffectData data)
    {
      ClientComponentStatusEffectColdManager.TargetIntensity = 0;
    }

    protected override void ClientUpdate(StatusEffectData data)
    {
      ClientComponentStatusEffectColdManager.TargetIntensity = data.Intensity;
    }

    protected override void PrepareEffects(Effects effects)
    {
      // add info to tooltip that this effect deals damage
      //effects.AddValue(this, StatName.VanityContinuousDamage, 1);
    }

    protected override void PrepareProtoStatusEffect()
    {
      base.PrepareProtoStatusEffect();

      // cache the tile offsets
      // but select only every third tile (will help to reduce the load without damaging accuracy too much)
      this.serverTileOffsetsCircle = ShapeTileOffsetsHelper
                                     .GenerateOffsetsCircle(EnvironmentalTileColdLookupAreaDiameter)
                                     .ToArray();

      this.serverTileOffsetsCircle = ShapeTileOffsetsHelper.SelectOffsetsWithRate(
          this.serverTileOffsetsCircle,
          rate: 3);

      this.serverProtoTileIce = Api.GetProtoEntity<TileIce>();
      this.serverProtoTileDarkIce = Api.GetProtoEntity<TileDarkIce>();
      this.serverProtoTileSnow = Api.GetProtoEntity<TileSnow>();
      this.serverProtoTileWaterSea = Api.GetProtoEntity<TileWaterSea>();
    }

    protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
    {
      // reduce directly applied status effect (e.g. from gunshots) based on armor (same as used for environmental effect)
      var defense = data.Character.SharedGetFinalStatValue(this.DefenseStatName);
      defense = MathHelper.Clamp(defense, 0, 1);
      intensityToAdd *= 1 - DefensePotentialMultiplier * defense;

      base.ServerAddIntensity(data, intensityToAdd);
    }

    protected override double ServerCalculateEnvironmentalIntensityAroundCharacter(ICharacter character)
    {
      var objectsEnviromentalIntensity = base.ServerCalculateEnvironmentalIntensityAroundCharacter(character);
      var tilesEnviromentalIntensity = this.ServerCalculateTileEnvironmentalIntensityAroundCharacter(character);
      return Math.Max(objectsEnviromentalIntensity, tilesEnviromentalIntensity);
    }

    protected override double ServerCalculateObjectEnvironmentalIntensity(
        ICharacter character,
        IWorldObject worldObject)
    {
      if (worldObject.ProtoWorldObject is not IProtoObjectColdSource protoColdSource)
      {
        return 0;
      }

      Vector2D position;
      switch (worldObject)
      {
        case IStaticWorldObject staticWorldObject:
          position = staticWorldObject.TilePosition.ToVector2D()
                     + staticWorldObject.ProtoStaticWorldObject.Layout.Center;
          break;
        case IDynamicWorldObject dynamicWorldObject:
          position = dynamicWorldObject.Position;
          break;
        default:
          throw new InvalidOperationException();
      }

      var distance = position.DistanceTo(character.Position);

      var maxDistance = protoColdSource.ColdRadiusMax;
      var minDistance = protoColdSource.ColdRadiusMin;
      var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
      var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);

      return intensity * MathHelper.Clamp(protoColdSource.ColdIntensity, 0, 1);
    }

    protected override void ServerUpdate(StatusEffectData data)
    {
      base.ServerUpdate(data);

      var damage = DamagePerSecondByIntensity
                   * Math.Pow(data.Intensity, 1.5)
                   * data.DeltaTime;

      // modify damage based on effect multiplier
      damage *= data.Character.SharedGetFinalStatMultiplier(StatName.ColdEffectMultiplier);

      // modify damage based on armor
      // divided by 2 because otherwise many armor pieces would give practically complete immunity to heat
      // so 100% armor would give 50% reduction in damage
      var defenseCold = data.Character.SharedGetFinalStatValue(StatName.DefenseCold);
      damage *= Math.Max(0, 1 - defenseCold / 2.0);

      data.CharacterCurrentStats.ServerReduceHealth(damage, data.StatusEffect);
    }

    // calculate closest ice tile position and heat intensity from it
    private double ServerCalculateTileEnvironmentalIntensityAroundCharacter(ICharacter character)
    {
      if (!character.ServerIsOnline)
      {
        return 0;
      }

      var tileSnowSessionIndex = this.serverProtoTileSnow.SessionIndex;
      var tileIceSessionIndex = this.serverProtoTileIce.SessionIndex;
      var tileDarkIceSessionIndex = this.serverProtoTileDarkIce.SessionIndex;
      var tileWaterSeaSessionIndex = this.serverProtoTileWaterSea.SessionIndex;

      var characterCurrentTileSessionIndex = character.Tile.ProtoTileSessionIndex;

      if (characterCurrentTileSessionIndex != tileSnowSessionIndex
          && characterCurrentTileSessionIndex != tileIceSessionIndex
          && characterCurrentTileSessionIndex != tileDarkIceSessionIndex
          && characterCurrentTileSessionIndex != tileWaterSeaSessionIndex)
      {
        // process ice cold only for players in snow biome
        return 0;
      }

      var characterTilePosition = character.TilePosition;
      var closestDistanceSqr = int.MaxValue;

      foreach (var tileOffset in this.serverTileOffsetsCircle)
      {
        var tilePosition = characterTilePosition.AddAndClamp(tileOffset);
        var tile = ServerWorld.GetTile(tilePosition, logOutOfBounds: false);
        if (!tile.IsValidTile
            || (tileIceSessionIndex != tile.ProtoTileSessionIndex
            && tileDarkIceSessionIndex != tile.ProtoTileSessionIndex))
        {
          continue;
        }

        var distanceSqr = tileOffset.X * tileOffset.X
                          + tileOffset.Y * tileOffset.Y;
        if (distanceSqr < closestDistanceSqr)
        {
          closestDistanceSqr = distanceSqr;
        }
      }

      if (closestDistanceSqr
          >= (EnvironmentalTileColdLookupAreaDiameter
              * EnvironmentalTileColdLookupAreaDiameter
              * 0.5
              * 0.5))
      {
        // no cold - too far from any ice
        return 0;
      }

      var coldIntensity = (EnvironmentalTileColdLookupAreaDiameter * 0.5 - Math.Sqrt(closestDistanceSqr))
                          / (EnvironmentalTileColdLookupAreaDiameter * 0.5);

      coldIntensity *= 2;
      coldIntensity = Math.Min(coldIntensity, 1);

      return coldIntensity;
    }
  }
}