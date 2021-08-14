namespace AtomicTorch.CBND.CoreMod.Characters
{
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Data.State;
  using AtomicTorch.GameEngine.Common.Primitives;
  using System.Collections.Generic;

  public class CharacterMobPrivateState : BaseCharacterPrivateState
  {
    [TempOnly]
    public double AttackRange { get; set; }

    [TempOnly]
    public ICharacter CurrentAggroCharacter { get; set; }

    [TempOnly]
    public double CurrentAggroTimeRemains { get; set; }

    [TempOnly]
    public ICharacter CurrentTargetCharacter { get; set; }

    [TempOnly]
    public List<Vector2D> CurrentTargetPosition { get; set; }

    [TempOnly]
    public bool IsRetreating { get; set; }

    [TempOnly]
    public double LastFleeSoundTime { get; set; }

    [TempOnly]
    public Vector2Ushort SpawnPosition { get; set; }

    /// <summary>
    /// This timer is incremented when the mob is too far from the spawn location.
    /// As it reaches a certain high number the mob should be despawned.
    /// </summary>
    [TempOnly]
    public double TimerDespawn { get; set; }


    public void SetCurrentTargetWithPosition(ICharacter target)
    {
      if(target is null)
      {
        CurrentTargetCharacter = null;
        CurrentTargetPosition = null;
        return;
      }

      if (CurrentTargetPosition == null)
        CurrentTargetPosition = new List<Vector2D>();

      if (CurrentTargetCharacter != target)
        CurrentTargetPosition.Clear();

      CurrentTargetCharacter = target;

      Vector2D position = target.Position;

      if (CurrentTargetPosition.Count >= 30)
        CurrentTargetPosition.RemoveAt(0);

      CurrentTargetPosition.Add(position);
    }
  }
}