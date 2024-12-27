using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneServerRestricted : ProtoZoneDefault
  {
    public static ZoneServerRestricted Instance { get; private set; }

    [NotLocalizable]
    public override string Name => "Server - Restricted";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      // Server restricted area
      Instance = this;

      foreach (var position in ServerZoneInstance.AllPositions)
      {
        var tile = Server.World.GetTile(position);
        if (!tile.IsValidTile)
          continue;

        foreach (var neighborTile in tile.EightNeighborTiles)
        {
          if (!ServerZoneInstance.IsContainsPosition(neighborTile.Position))
          {
            var physicsBody = Server.World.CreateStandalonePhysicsBody(position.ToVector2D());
            physicsBody.AddShapeRectangle((1, 1), null, CollisionGroups.Water);
            physicsBody.AddShapeRectangle((1, 1), null, CollisionGroups.HoverWater);
            Server.World.AddStandalonePhysicsBody(physicsBody, Server.World.GetPhysicsSpace());
            break;
          }
        }
      }
    }


  }
}