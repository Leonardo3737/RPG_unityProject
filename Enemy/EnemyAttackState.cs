using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
  private static List<Attack<EnemyStateMachine>> CurrentAttacks;
  private static readonly List<Attack<EnemyStateMachine>> MainAttacks = new()
        {
          new(Animator.StringToHash("Attack-1"), 0.01f, 0.55f),
          new(Animator.StringToHash("Attack-2"), 0.2f, 0.46f),
        };

  /* private static readonly List<Attack<EnemyStateMachine>> AlternativeAttacks = new()
  {
    new(Animator.StringToHash("AlternativeAttack-2"), 0.2f, 0.6f),
    new(Animator.StringToHash("AlternativeAttack-1"), 0.3f, 0.6f, AttackActions<EnemyStateMachine>.AlternativeAttack1Action),
    new(Animator.StringToHash("AlternativeAttack-3"), 0.15f, 1f, AttackActions<EnemyStateMachine>.AlternativeAttack3Action),
  }; */

  //private static readonly int RunToAttackAnimation = Animator.StringToHash("RunToAttack");

  private Attack<EnemyStateMachine> Attack;

  public EnemyAttackState(
    EnemyStateMachine stateMachine,
    AttackTypes AttackType
    ) : base(stateMachine, StatesType.ATTACK)
  {
    CurrentAttacks = MainAttacks;
  }

  public override void Enter()
  {
    Attack = CurrentAttacks[sm.AttackIndex];
    
    sm.AttackIndex++;

    if (CurrentAttacks.Count < sm.AttackIndex + 1)
    {
      sm.AttackIndex = 0;
    }

    sm.Animator.CrossFadeInFixedTime(Attack.AnimationName, 0.1f, 0, Attack.Start);
  }

  public override void Update(float deltaTime)
  {

    var normalizedTime = GetNormalizedTime(sm.Animator, "attack");

    // REALIZA DO ATAQUE AÇÃO CASO TENHA
    Attack.Action?.Invoke(sm, deltaTime, normalizedTime);

    if (normalizedTime >= 1f)
    {
      EndCombo();
    }
  }

  public override void Exit()
  {
    if (sm.CancelAttack)
    {
      sm.Invoke("ResetCancelAttack", 0.2f);
    }
  }

  public override bool CanPerformAction()
  {
    var animationTime = GetNormalizedTime(sm.Animator, "attack");

    return animationTime >= Attack.CanMatch || sm.CancelAttack;
  }

  public void EndCombo()
  {
    sm.ChangeState(new EnemyCombatState(sm));
  }
}