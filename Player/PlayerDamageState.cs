using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageState : PlayerBaseState
{
  private int Damage;

  private int DamageAnimation;
  private float Timer;

  private DamageAction Action;

  public PlayerDamageState(
    PlayerStateMachine stateMachine,
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
    sm.VoiceAudioSource.PlayOneShot(sm.DamageSounds[Random.Range(0, sm.DamageSounds.Length)]);
    sm.Animator.CrossFadeInFixedTime(DamageAnimation, 0.1f, sm.CurrentLayer);
    sm.CurrentHealth -= Damage;

    sm.HealthImage.fillAmount = sm.CurrentHealth > 0 ? sm.CurrentHealth / sm.MaxHealth : 0;
  }

  public override void Update(float deltaTime)
  {
    Timer += deltaTime;
    if (Timer > 1f)
    {
      if (sm.IsTriggered)
      {
        sm.ChangeState(new PlayerTriggerState(sm));
      }
      else
      {
        sm.ChangeState(new PlayerFreeLookState(sm));
      }
      return;
    }
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
    return false;
  }

  public void End()
  {

  }

}