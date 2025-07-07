using Unity.Cinemachine;
using UnityEngine;

public class PlayerTriggerState : PlayerBaseState
{
  private EnemyStateMachine Target;

  public PlayerTriggerState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.TRIGGER)
  {
  }

  public override void Enter()
  {
    Target = sm.Targeter.CurrentTarget;

    var input = sm.InputHandler.InputMovement.normalized;

    float velocityX = CurrentAnimationSmooth * input.x;
    float velocityZ = CurrentAnimationSmooth * input.y;

    sm.Animator.CrossFadeInFixedTime(TriggerBlendTree, 0.1f, sm.CurrentLayer);

    sm.Animator.SetFloat(TriggerSpeedX, velocityX, 1f, Time.deltaTime);
    sm.Animator.SetFloat(TriggerSpeedZ, velocityZ, 1f, Time.deltaTime);

    if (Target != null && Target.FocusIndicatorImage != null)
    {
      Target.FocusIndicatorImage.color = Colors.TransparentRed;
    }
  }

  public override void Update(float deltaTime)
  {
    Target = sm.Targeter.CurrentTarget;

    if (sm.IsChangingTarget) return;

    var IsChecked = CheckTarget();

    if (!IsChecked) return;

    if (sm.FreeLookCamera.TryGetComponent(out CinemachineInputAxisController inputController))
    {
      inputController.enabled = false;
    }

    TriggerMovement(deltaTime);
  }

  public override void Exit()
  {
    if (Target != null)
    {
      Target.IsBeingFocused = false;
    }
  }

  public override bool CanPerformAction()
  {
    return true;
  }
}