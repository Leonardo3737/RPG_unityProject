using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
  private float PatrolRadius = 7f;
  private float WaitTimeAtPoint = 2f;

  private Vector3 OriginPosition;
  private float WaitTimer = 0f;
  private float JustChasedTimer = 0f;
  private bool Waiting = false;

  public EnemyPatrolState(EnemyStateMachine stateMachine) : base(stateMachine, StatesType.PATROL) { }


  public override void Enter()
  {

    OriginPosition = sm.transform.position;

    sm.NavMeshAgent.speed = sm.PatrolSpeed;

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.ChaseSpeed;

    sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f);
    sm.Animator.SetFloat(FreeLookSpeed, animationSpeed);

    if (sm.IsPatrol)
    {
      SetNewDestination();
    }
  }

  public override void Update(float deltaTime)
  {
    if (sm.JustChased)
    {
      JustChasedTimer += Time.deltaTime;

      if (JustChasedTimer > 2f)
      {
        sm.JustChased = false;
      }
    }

    CheckIsPlayerVisible();

    if (HasReachedDestination() && sm.IsPatrol)
    {
      if (!Waiting)
      {
        Waiting = true;
        WaitTimer = 0f;
      }

      WaitTimer += Time.deltaTime;

      if (WaitTimer >= WaitTimeAtPoint)
      {
        Waiting = false;
        SetNewDestination();
      }
    }

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.ChaseSpeed;

    RunAnimation(deltaTime, animationSpeed);
  }

  private void SetNewDestination()
  {
    Vector3 randomPoint = GetRandomPointInNavMesh(OriginPosition, PatrolRadius);
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
    Gizmos.DrawWireSphere(sm.transform.position, PatrolRadius);
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