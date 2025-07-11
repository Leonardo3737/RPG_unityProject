

using UnityEngine;
using UnityEngine.Rendering;

public class PlayerDefendState : PlayerBaseState
{
  private int AnimationName = Animator.StringToHash("Center Block");
  private float NormalizedTime;
  private float Start = 0.36f;
  private float End = 0.7f;
  public PlayerDefendState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.DEFEND) { }

  public override void Enter()
  {
    sm.Animator.CrossFadeInFixedTime(AnimationName, 0.1f, sm.CurrentLayer);
  }

  public override void Update(float deltaTime)
  {
    NormalizedTime = GetNormalizedTime(sm.Animator, "defense", sm.CurrentLayer);

    if (NormalizedTime > Start && NormalizedTime < End && !sm.IsDefending)
    {
      sm.IsDefending = true;
    }
    if (NormalizedTime > End && sm.IsDefending)
    {
      sm.IsDefending = false;
    }

    if (NormalizedTime > 1f)
    {
      if (sm.IsTriggered)
      {
        sm.ChangeState(new PlayerTriggerState(sm));
      }
      else
      {
        sm.ChangeState(new PlayerFreeLookState(sm));
      }
    }
  }

  public override void Exit()
  {
    if (sm.IsDefending)
    {
      sm.IsDefending = false;
    }
  }

  public override bool CanPerformAction()
  {
    return NormalizedTime > 0.47f;
  }
}