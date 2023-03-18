using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
using AtomicTorch.CBND.CoreMod.Tiles;
using AtomicTorch.CBND.CoreMod.Triggers;
using AtomicTorch.CBND.GameApi.Data;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Data.Zones;
using AtomicTorch.GameEngine.Common.Primitives;
using System;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class SpawnLootWater : ProtoZoneSpawnScript
  {
    protected override double MaxSpawnAttemptsMultiplier => 10;

    protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
    {
      triggers
          // trigger on world init
          .Add(GetTrigger<TriggerWorldInit>())
          // trigger on time interval
          .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(30)));

      var presetPragmiumSource = spawnList.CreatePreset(interval: 50, padding: 2, useSectorDensity: false)
                                          .Add<ObjectLootCrateSpaceshipLost>()
                                          .SetCustomPaddingWithSelf(79);

      // special restriction preset for player land claims
      var restrictionPresetLandclaim = spawnList.CreateRestrictedPreset()
                                                .Add<IProtoObjectLandClaim>();

      // Let's ensure that we don't spawn too close to players' buildings.
      // take half size of the largest land claim area
      var paddingToLandClaimsSize = LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value / 2.0 + 6;

      presetPragmiumSource.SetCustomPaddingWith(restrictionPresetLandclaim, paddingToLandClaimsSize);
    }

    protected override IGameObjectWithProto ServerSpawnStaticObject(
        IProtoTrigger trigger,
        IServerZone zone,
        IProtoStaticWorldObject protoStaticWorldObject,
        Vector2Ushort tilePosition)
    {
      var hasBadNeighborTile = false;

      foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
      {
        if (tileOffset.X == 0
            && tileOffset.Y == 0)
        {
          continue;
        }

        var tile = Server.World.GetTile(tilePosition.X + 2 * tileOffset.X,
                                        tilePosition.Y + 2 * tileOffset.Y);

        foreach (var neighborTile in tile.EightNeighborTiles)
        {
          if (neighborTile.ProtoTile is TileDarkIce || neighborTile.ProtoTile.Kind == TileKind.Solid)
          {
            hasBadNeighborTile = true;
            break;
          }
        }

        if (hasBadNeighborTile)
          break;
      }

      if (hasBadNeighborTile)
        return null;

      return base.ServerSpawnStaticObject(trigger, zone, protoStaticWorldObject, tilePosition);
    }

  }
}
