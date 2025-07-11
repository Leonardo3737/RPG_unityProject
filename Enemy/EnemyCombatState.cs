using UnityEngine;

public class EnemyCombatState : EnemyBaseState
{

  public EnemyCombatState(EnemyStateMachine stateMachine) : base(stateMachine, StatesType.COMBAT) { }

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
    var target = sm.Targeter.CurrentTarget;
    if (!sm.Targeter.SelectTarget() && target == null && HasReachedDestination())
    {
      sm.ChangeState(new EnemyPatrolState(sm));
      return;
    }

    var distance = Vector3.Distance(sm.transform.position, target.transform.position);

    if (distance > 7f)
    {
      sm.ChangeState(new EnemyChaseState(sm));
      return;
    }

    sm.NavMeshAgent.destination = target.transform.position;


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