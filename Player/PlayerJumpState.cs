

using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
  private bool IsRunning;
  private bool IsStartJumping;
  private bool Aux;
  private bool IsEndJumping;
  private float JumpStartTime;
  //private float JumpEndTime;
  private float JumpHeight;
  private Vector3 Direction;
  public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.JUMP) { }


  public override void Enter()
  {
    Direction = FaceInputDirectionInstantly();

    //Move(Time.deltaTime, Direction);

    IsRunning = Direction != Vector3.zero;
    JumpHeight = IsRunning ? 0.2f : 0.15f;

    JumpStartTime = IsRunning ? 0.07f : 0.26f;
    /* if (sm.CurrentMode == Modes.UNARMED)
    {
      JumpStartTime = IsRunning ? 0.07f : 0.26f;
      //JumpEndTime = IsRunning ? 0.75f : 0.54f;
    }
    else
    {
      JumpStartTime = IsRunning ? 0.27f : 0.22f;
      //JumpEndTime = IsRunning ? 0.44f : 0.36f;
    } */

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
    else if (!IsEndJumping)
    {
      sm.ForceReceiver.VerticalVelocity = 0;
      IsEndJumping = true;
    }

    Move(deltaTime, Direction);

    if (sm.Controller.isGrounded && normalizedTime > JumpStartTime && !Aux)
    {
      Direction = FaceInputDirectionInstantly();
      if (
          (sm.InputHandler.InputMovement == Vector2.zero && IsRunning) ||
          (sm.InputHandler.InputMovement != Vector2.zero && !IsRunning)
        )
      {
        sm.ChangeState(new PlayerFreeLookState(sm));
      }
      //Aux = true;
      //Debug.Log(normalizedTime);
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