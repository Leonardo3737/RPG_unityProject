using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
  public EnemyPatrolState(EnemyStateMachine stateMachine) : base(stateMachine) { }

  private float AnimationSpeed;
  private float patrolRadius = 7f;
  private float waitTimeAtPoint = 2f;

  private Vector3 originPosition;
  private float waitTimer = 0f;
  private bool waiting = false;

  public override void Enter()
  {
    StateType = StatesType.PATROL;

    originPosition = sm.transform.position;

    sm.NavMeshAgent.speed = sm.PatrolSpeed;

    sm.NavMeshAgent.updateRotation = false;

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.PursuitSpeed;

    sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f);
    sm.Animator.SetFloat(FreeLookSpeed, animationSpeed);

    if (sm.IsPatrol)
    {
      SetNewDestination();
    }
  }

  public override void Update(float deltaTime)
  {
    if (sm.Targeter.SelectTarget() && sm.Targeter.HasLineOfSight() && sm.IsPatrol)
    {
      sm.ChangeState(new EnemyPursuitState(sm));
      return;
    }
    if (!sm.NavMeshAgent.pathPending && sm.NavMeshAgent.remainingDistance <= sm.NavMeshAgent.stoppingDistance && sm.IsPatrol)
    {
      if (!waiting)
      {
        waiting = true;
        waitTimer = 0f;
      }

      waitTimer += Time.deltaTime;

      if (waitTimer >= waitTimeAtPoint)
      {
        waiting = false;
        SetNewDestination();
      }
    }
    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.PursuitSpeed;
    RunAnimation(deltaTime, animationSpeed);
    FaceMoveDirection(deltaTime);

  }

  private void SetNewDestination()
  {
    Vector3 randomPoint = GetRandomPointInNavMesh(originPosition, patrolRadius);
    sm.NavMeshAgent.SetDestination(randomPoint);
  }

  private Vector3 GetRandomPointInNavMesh(Vector3 center, float radius)
  {
    for (int i = 0; i < 30; i++)
    {
      Vector3 randomDir = Random.insideUnitSphere * radius;
      randomDir.y = 0; // manter no plano

      Vector3 candidate = center + randomDir;

      if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 4f, NavMesh.AllAreas))
      {
        return hit.position;
      }
    }

    return center; // fallback
  }

  // Gizmo para visualizar a área de patrulha no Editor
  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(sm.transform.position, patrolRadius);
  }

  public override void Exit()
  {
    sm.NavMeshAgent.ResetPath();
  }

  public override bool CanPerformAction()
  {
    return true;
  }
}