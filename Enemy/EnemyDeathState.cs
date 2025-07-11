using UnityEngine;

public class EnemyDeathState : EnemyBaseState
{

  private readonly int DeathAnimator = Animator.StringToHash("Dying");

  public EnemyDeathState(EnemyStateMachine stateMachine) : base(stateMachine, StatesType.DEATH) { }

  public override void Enter()
  {

    sm.Animator.CrossFadeInFixedTime(DeathAnimator, 0.1f);
    sm.SetIsDie();
  }

  public override void Update(float deltaTime)
  {
    var normalizedTime = GetNormalizedTime(sm.Animator, "Death");

    if (normalizedTime > 1f)
    {
      sm.Die();
    }
  }

  public override void Exit()
  {

  }

  public override bool CanPerformAction()
  {
    return false;
  }
}