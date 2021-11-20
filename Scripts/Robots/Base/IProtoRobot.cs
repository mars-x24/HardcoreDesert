namespace AtomicTorch.CBND.CoreMod.Robots
{
  using AtomicTorch.CBND.CoreMod.Items.Robots;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Data.World;

  public interface IProtoRobot : IProtoDynamicWorldObject
  {
    IProtoItemRobot ProtoItemRobot { get; }

    double StatMoveSpeed { get; }

    void ServerDropRobotToGround(
        IDynamicWorldObject objectRobot,
        Tile tile,
        ICharacter forOwnerCharacter);

    IItemsContainer ServerGetStorageItemsContainer(IDynamicWorldObject objectRobot);

    void ServerOnRobotDroppedOrReturned(
        IDynamicWorldObject objectRobot,
        ICharacter toCharacter,
        bool isReturnedToPlayer);

    void ServerSetupAssociatedItem(
        IDynamicWorldObject objectRobot,
        IItem item);

    void ServerStartRobot(
        IDynamicWorldObject objectRobot,
        IWorldObject owner,
        IItemsContainer ownerContainer);
  }
}