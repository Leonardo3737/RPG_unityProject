using UnityEngine;

public class EnemyInvestigateState : EnemyBaseState
{
  private Vector3 Origin;

  public EnemyInvestigateState(
    EnemyStateMachine stateMachine,
    Vector3 origin
    ) : base(stateMachine, StatesType.INVESTIGATE)
  {
    Origin = origin;
  }

  public override void Enter()
  {

    Vector3 directionToOrigin = Origin - sm.transform.position;
    directionToOrigin.y = 0;

    sm.NavMeshAgent.destination = Origin - (directionToOrigin * 0.3f);

    Debug.Log(PreviousStateType.ToString());

    sm.NavMeshAgent.speed = sm.JustChased ? sm.ChaseSpeed : sm.PatrolSpeed;

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.ChaseSpeed;

    sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f);
    sm.Animator.SetFloat(FreeLookSpeed, animationSpeed);
  }

  public override void Update(float deltaTime)
  {
    CheckIsPlayerVisible();

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.ChaseSpeed;

    RunAnimation(deltaTime, animationSpeed);

    if (HasReachedDestination())
    {
      sm.ChangeState(new EnemyPatrolState(sm));
    }
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