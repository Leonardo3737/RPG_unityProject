using UnityEngine;

public class EnemyPursuitState : EnemyBaseState
{

  public EnemyPursuitState(EnemyStateMachine stateMachine) : base(stateMachine) { }

  public override void Enter()
  {
    StateType = StatesType.PURSUIT;

    sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f);
    sm.Animator.SetFloat(FreeLookSpeed, 0f);
    sm.NavMeshAgent.speed = sm.PursuitSpeed;
  }

  public override void Update(float deltaTime)
  {

    var target = sm.Targeter.CurrentTarget;
    if (target != null)
    {
      var distance = Vector3.Distance(target.transform.position, sm.transform.position);
      if (distance > 2f)
      {
        sm.NavMeshAgent.destination = target.transform.position;
      }
      else if(sm.NavMeshAgent.hasPath)
      {
        sm.NavMeshAgent.ResetPath();
      }
    }

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.PursuitSpeed;
    RunAnimation(deltaTime, animationSpeed);
    FaceMoveDirection(deltaTime);

    if (!sm.Targeter.SelectTarget() || !sm.Targeter.HasLineOfSight())
    {
      sm.ChangeState(new EnemyPatrolState(sm));
      return;
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