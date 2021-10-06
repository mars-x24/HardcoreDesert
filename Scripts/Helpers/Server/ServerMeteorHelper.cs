namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
  using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
  using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Helpers;
  using AtomicTorch.GameEngine.Common.Primitives;

  public static class ServerMeteorHelper
  {
    public static void Start(Vector2Ushort circleCenterPosition, ushort radius, int totalSeconds, int meteorPerSecond)
    {
      int totalMeteors = totalSeconds * meteorPerSecond;

      do
      {
        var delay = RandomHelper.Next(totalSeconds);
        ServerTimersSystem.AddAction(delay, () => SpawnMeteor(circleCenterPosition, radius));
        totalMeteors--;
      }
      while (totalMeteors > 0);
    }

    private static void SpawnMeteor(Vector2Ushort circleCenterPosition, ushort radius)
    {
      bool positionFound = false;
      var randomPosition = circleCenterPosition;

      var attemps = 100;
      do
      {
        randomPosition = SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(circleCenterPosition, radius);

        RectangleInt rect = new RectangleInt(randomPosition.X - 5, randomPosition.Y - 5, 10, 10);
        if (!LandClaimSystem.SharedIsLandClaimedByAnyone(rect))
          positionFound = true;
        else
          attemps--;
      }
      while (attemps > 0 && !positionFound);

      if (positionFound)
      {
        var meteor = Api.GetProtoEntity<ObjectMeteorExplosion>();
        Api.Server.World.CreateStaticWorldObject(meteor, randomPosition);
      }
    }

  }
}