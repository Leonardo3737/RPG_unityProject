

using UnityEngine;
using UnityEngine.Rendering;

public class PlayerJumpState : PlayerBaseState
{
  private bool IsRunning;
  private bool IsStartJumping;
  private float JumpStartTime;
  //private float JumpEndTime;
  private float JumpHeight;
  private Vector3 Direction;
  public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.JUMP) { }


  public override void Enter()
  {
    Direction = FaceInputDirection();
    if (sm.IsTriggered)
    {
      sm.IsTriggered = false;
    }

    IsRunning = Direction != Vector3.zero;
    JumpHeight = IsRunning ? 0.2f : 0.15f;

    JumpStartTime = IsRunning ? 0.07f : 0.26f;

    var animation = !IsRunning ? Animator.StringToHash("Jump") : Animator.StringToHash("JumpRunning");

    sm.Animator.CrossFadeInFixedTime(animation, 0.1f, sm.CurrentLayer);
  }

  public override void Update(float deltaTime)
  {
    if (sm.Animator.IsInTransition(sm.CurrentLayer))
    {
      Move(deltaTime, Direction);
      return;
    }
    var normalizedTime = GetNormalizedTime(sm.Animator, "jump", sm.CurrentLayer);

    if (normalizedTime > JumpStartTime && !IsStartJumping)
    {
      float jumpForce = Mathf.Sqrt(2f * -Physics.gravity.y * JumpHeight);
      sm.ForceReceiver.VerticalVelocity = jumpForce;
      IsStartJumping = true;
    }

    Move(deltaTime, Direction);

    if (sm.Controller.isGrounded && normalizedTime > JumpStartTime)
    {
      Direction = FaceInputDirection(true, deltaTime);
      if (
          (sm.InputHandler.InputMovement == Vector2.zero && IsRunning) ||
          (sm.InputHandler.InputMovement != Vector2.zero && !IsRunning)
        )
      {
        sm.ChangeState(new PlayerFreeLookState(sm));
      }
    }

    if (normalizedTime > 1f)
    {
      sm.ChangeState(new PlayerFreeLookState(sm));
    }
  }

  public override void Exit()
  {
    sm.IsJumping = false;
  }

  public override bool CanPerformAction()
  {
    return false;
  }
}