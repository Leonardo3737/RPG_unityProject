using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{

  public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine, StatesType.CHASE) { }

  public override void Enter()
  {
    sm.NavMeshAgent.speed = sm.ChaseSpeed;

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.ChaseSpeed;

    sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f);
    sm.Animator.SetFloat(FreeLookSpeed, animationSpeed);
  }

  public override void Update(float deltaTime)
  {

    if (CanAttack())
    {
      sm.NavMeshAgent.ResetPath();
      sm.NavMeshAgent.velocity = Vector3.zero;
      sm.ChangeState(new EnemyAttackState(sm, AttackTypes.MainAttack));
      return;
    }
    if ((!sm.Targeter.SelectTarget() || !sm.Targeter.HasLineOfSight()) && HasReachedDestination())
    {
      sm.ChangeState(new EnemyPatrolState(sm));
      return;
    }

    var target = sm.Targeter.CurrentTarget;
    if (target != null)
    {
      sm.NavMeshAgent.destination = target.transform.position;
    }

    var currentSpeed = sm.NavMeshAgent.velocity.magnitude;
    var animationSpeed = currentSpeed / sm.ChaseSpeed;
    RunAnimation(deltaTime, animationSpeed);
  }

  public override void Exit()
  {
    sm.JustChased = true;
    sm.NavMeshAgent.ResetPath();
  }

  public override bool CanPerformAction()
  {
    return true;
  }
}