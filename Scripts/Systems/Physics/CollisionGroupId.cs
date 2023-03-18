using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Systems.Physics
{
  [RemoteEnum]
  public enum CollisionGroupId : byte
  {
    Default = 0,

    HitboxMelee = 1,

    HitboxRanged = 2,

    ClickArea = 3,

    InteractionArea = 4,

    Water = 5,

    HoverWater = 6
  }
}