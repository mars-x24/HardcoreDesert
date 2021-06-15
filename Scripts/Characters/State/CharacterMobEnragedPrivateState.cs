namespace AtomicTorch.CBND.CoreMod.Characters
{
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.CBND.GameApi.Data.World;
  using AtomicTorch.GameEngine.Common.Primitives;

  public class CharacterMobEnragedPrivateState : CharacterMobPrivateState
  {
    [TempOnly]
    public IStaticWorldObject CurrentAggroStructure { get; set; }


    [TempOnly]
    public Vector2Ushort LastPosition { get; set; }
  }
}