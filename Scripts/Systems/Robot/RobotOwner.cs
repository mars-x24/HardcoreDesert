using AtomicTorch.CBND.GameApi.Data;
using AtomicTorch.CBND.GameApi.Data.Items;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.GameEngine.Common.Primitives;

namespace HardcoreDesert.Scripts.Systems.Robot
{
  public struct RobotOwner
  {
    public IWorldObject Owner;
    public IItemsContainer OwnerContainer;
    public Vector2Ushort Position;
    public IProtoEntity TargetItemProto;
    public IItem RobotItem;
    public IDynamicWorldObject RobotObject;
    public RobotItemHelper RobotItemHelper;
  }
}
