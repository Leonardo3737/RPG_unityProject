using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageState : EnemyBaseState
{
  private int Damage;

  private int DamageAnimation;

  private DamageAction Action;

  public EnemyDamageState(
    EnemyStateMachine stateMachine,
    int Damage,
    string DamageAnimationName,
    DamageAction Action
    ) : base(stateMachine, StatesType.DAMAGE)
  {
    this.Damage = Damage;
    DamageAnimation = Animator.StringToHash(DamageAnimationName);
    this.Action = Action;
  }

  public override void Enter()
  {
    sm.NavMeshAgent.ResetPath();
    sm.NavMeshAgent.velocity = Vector3.zero;
    
    sm.VoiceAudioSource.PlayOneShot(sm.DamageSounds[Random.Range(0, sm.DamageSounds.Length)]);
    sm.Animator.CrossFadeInFixedTime(DamageAnimation, 0.1f);
    sm.CurrentHealth -= Damage;
    sm.HealthImage.fillAmount = sm.CurrentHealth > 0 ? sm.CurrentHealth / sm.MaxHealth : 0;


  }

  public override void Update(float deltaTime)
  {
    var animationTime = GetNormalizedTime(sm.Animator, "damage");

    if (Action != null)
    {
      Action.Run(deltaTime, animationTime, sm);
    }

    if (animationTime >= 1f)
    {
      End();
      return;
    }
  }

  public override void Exit()
  {

  }
  public override bool CanPerformAction()
  {
    return true;
  }

  public void End()
  {
    sm.ChangeState(new EnemyCombatState(sm));
  }

}