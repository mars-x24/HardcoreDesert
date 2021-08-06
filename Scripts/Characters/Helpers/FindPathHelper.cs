using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.GameEngine.Common.Primitives;
using System;

namespace AtomicTorch.CBND.CoreMod.Characters
{
  public class FindPathHelper : IDisposable
  {
    private ICharacter characterNpc, enemyCharacter;
    private Vector2D characterNpcPosition;
    private float npcWeaponOffset, enemyWeaponOffset;
    private int maxIterations, iteration;

    public bool PathFound
    {
      get { return this.pathFound; }
    }
    private bool pathFound = true;

    public FindPathHelper(ICharacter characterNpc, Vector2D characterNpcPosition, ICharacter enemyCharacter, float npcWeaponOffset, float enemyWeaponOffset, int maxIterations, int iteration = 1)
    {
      this.characterNpc = characterNpc;
      this.enemyCharacter = enemyCharacter;

      this.characterNpcPosition = characterNpcPosition;

      this.npcWeaponOffset = npcWeaponOffset;
      this.enemyWeaponOffset = enemyWeaponOffset;

      this.maxIterations = maxIterations;
      this.iteration = iteration;
    }

    public void FindPathToEnemy(
        out double distanceToOriginalTarget,
        out double distanceToTarget,
        out Vector2F directionToTargetPosition,
        out Vector2F directionToTargetHitbox,
        out bool hasObstacles,
        out bool destinationIsEnemy,
        bool tryOtherPath = false
      )
    {
      SetDistanceTo(characterNpcPosition, npcWeaponOffset, enemyCharacter.Position, enemyWeaponOffset, out distanceToTarget, out directionToTargetPosition, out directionToTargetHitbox);

      distanceToOriginalTarget = distanceToTarget;

      var tempOriginalDistanceToTarget = distanceToTarget;
      var tempOriginalDirectionToTargetPosition = directionToTargetPosition;
      var tempOriginalDirectionToTargetHitbox = directionToTargetHitbox;

      hasObstacles = !FindPathToEnemy(0.0f, tempOriginalDirectionToTargetHitbox, ref tempOriginalDistanceToTarget, ref directionToTargetPosition, ref directionToTargetHitbox);

      distanceToTarget = tempOriginalDistanceToTarget;
      directionToTargetPosition = tempOriginalDirectionToTargetPosition;
      directionToTargetHitbox = tempOriginalDirectionToTargetHitbox;

      if (tryOtherPath && hasObstacles)
      {
        for (int angle = 30; angle <= 359; angle += 30)
        {
          pathFound = FindPathToEnemy(angle, tempOriginalDirectionToTargetHitbox, ref distanceToTarget, ref directionToTargetPosition, ref directionToTargetHitbox);

          if (pathFound)
            break;

          distanceToTarget = tempOriginalDistanceToTarget;
          directionToTargetPosition = tempOriginalDirectionToTargetPosition;
          directionToTargetHitbox = tempOriginalDirectionToTargetHitbox;
        }
      }

      destinationIsEnemy = directionToTargetHitbox == tempOriginalDirectionToTargetHitbox;
    }

    private bool FindPathToEnemy(float deg, Vector2F originalDirectionToEnemyHitbox, ref double distanceToEnemy, ref Vector2F directionToEnemyPosition, ref Vector2F directionToEnemyHitbox)
    {
      //Find another path
      Vector2F v = (originalDirectionToEnemyHitbox * 2).RotateDeg(deg);

      if (!HasObstaclesInTheWay(characterNpc, enemyCharacter, characterNpcPosition, enemyCharacter.Position, sendDebugEvents: false))
        return true;

      //Try along the way
      for (int i = 4; i > 0; i--)
      {
        var alongWayPosition = characterNpc.Position + (v / i);

        if (!HasObstaclesInTheWay(characterNpc, enemyCharacter, characterNpcPosition, alongWayPosition, sendDebugEvents: false))
        {
          SetDistanceTo(characterNpcPosition, npcWeaponOffset, alongWayPosition, 0.0, out distanceToEnemy, out directionToEnemyPosition, out directionToEnemyHitbox);

          if (!HasObstaclesInTheWay(characterNpc, enemyCharacter, alongWayPosition, enemyCharacter.Position, sendDebugEvents: false))
            return true;

          if (this.iteration < this.maxIterations)
          {
            bool done = false;

            using (FindPathHelper findPathHelper = new FindPathHelper(characterNpc, alongWayPosition, enemyCharacter, npcWeaponOffset, enemyWeaponOffset, this.maxIterations, this.iteration + 1))
            {
              findPathHelper.FindPathToEnemy(
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                true);

              done = findPathHelper.PathFound;
            }

            if (done)
              return true;
          }
        }
      }
       
      return false;
    }

    private void SetDistanceTo(Vector2D positionFrom, double positionFromOffset, Vector2D positionTo, double positionToOffset, out double distanceToEnemy, out Vector2F directionToEnemyPosition, out Vector2F directionToEnemyHitbox)
    {
      var deltaPos = positionTo - positionFrom;
      directionToEnemyPosition = (Vector2F)deltaPos;

      deltaPos = (deltaPos.X, deltaPos.Y + positionToOffset - positionFromOffset);

      distanceToEnemy = deltaPos.Length;
      directionToEnemyHitbox = (Vector2F)deltaPos;
    }

    private bool HasObstaclesInTheWay(ICharacter characterNpc, ICharacter enemyCharacter, Vector2D fromPosition, Vector2D toPosition, bool sendDebugEvents)
    {
      var tempLineTestResults = characterNpc.PhysicsBody.PhysicsSpace.TestLine(fromPosition, toPosition, CollisionGroups.Default, sendDebugEvents);
      var tempLineTestResultsUp = characterNpc.PhysicsBody.PhysicsSpace.TestLine(fromPosition + (0, npcWeaponOffset), toPosition + (0, enemyWeaponOffset), CollisionGroups.Default, sendDebugEvents);

      tempLineTestResults.AddRange(tempLineTestResultsUp.AsList());

      foreach (var testResult in tempLineTestResults.AsList())
      {
        var worldObject = testResult.PhysicsBody.AssociatedWorldObject;
        if (ReferenceEquals(worldObject, characterNpc))
          continue;
        if (ReferenceEquals(worldObject, enemyCharacter))
          continue;
        if (worldObject.ProtoGameObject is IProtoCharacter)
          continue;
        if (worldObject is not null)
          return true;
      }

      return false;
    }

    public void Dispose()
    {

    }
  }
}
