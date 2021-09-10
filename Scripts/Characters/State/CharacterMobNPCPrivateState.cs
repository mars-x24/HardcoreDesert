using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.Characters
{
  public class CharacterMobNPCPrivateState : CharacterMobPrivateState
  {
    [TempOnly]
    public bool IsReloading { get; set; }

    public double ReloadTimer { get; set; }

    public double AmmosFired { get; set; }

  }
}