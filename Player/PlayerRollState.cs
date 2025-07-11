using Unity.Cinemachine;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
  private static readonly int RollAnimationName = Animator.StringToHash("Roll");
  private bool isFirstRender = true;
  private Vector3 startLerpPosition;
  private Vector3 endLerpPosition;
  private bool isLerpingBack = false;
  private float lerpProgress = 0f;
  public PlayerRollState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.ROLL) { }

  public override void Enter()
  {
    FaceInputDirection();

    sm.Animator.CrossFadeInFixedTime(RollAnimationName, 0.1f, sm.CurrentLayer);
    sm.Controller.height = sm.CrouchedControllerHeigth;
  }

  public override void Update(float deltaTime)
  {

    if (sm.IsTriggered)
    {
      TriggerPositionCamera(deltaTime);
      CameraLookTarget();
    }

    else if ((sm.FreeLookCamera.TryGetComponent(out CinemachineInputAxisController inputController) && !inputController.enabled) || !isFirstRender)
    {
      if (isFirstRender)
      {
        StartCameraTargetReturn();
        isFirstRender = false;
      }
      else
      {
        PositionCameraTarget(deltaTime);
      }

      inputController.enabled = true;

    }

    FreeLookMove(deltaTime, Vector3.zero);

    var animationTime = GetNormalizedTime(sm.Animator, "roll");

    /* if (sm.CancelAttack && animationTime > 0.3f)
    {
      sm.OnCancelAttack();
      sm.CancelAttack = false;
    } */

    float jumpDuration = 0.65f;


    if (animationTime < jumpDuration)
    {
      Vector3 move = sm.transform.forward * (sm.RollSpeed * deltaTime);
      sm.Controller.Move(move);
    }
    else
    {
      sm.Controller.Move(sm.transform.forward * (sm.RollSpeed * 0.4f * Time.deltaTime));
    }

    if (animationTime >= 1f)
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
    sm.JustRolled = true;
    sm.Controller.height = sm.RegularControllerHeigth;
  }

  public override bool CanPerformAction()
  {
    var animationTime = GetNormalizedTime(sm.Animator, "roll");

    return animationTime >= 0.85f;
  }

  private void CameraLookTarget()
  {
    if (sm.Targeter.SelectTarget() && sm.Targeter.HasLineOfSight())
    {
      var targetPosition = sm.Targeter.GetTargetPosition();

      sm.CameraTarget.transform.position = (Vector3)targetPosition;
    }
  }

  private void StartCameraTargetReturn()
  {

    if (PreviousStateType == StatesType.TRIGGER)
    {
      startLerpPosition = sm.Targeter.GetTargetPosition().Value;
    }
    else
    {
      startLerpPosition = sm.CameraTarget.transform.position;
    }
    endLerpPosition = sm.transform.position + new Vector3(0, sm.CameraTargetHeight);
    isLerpingBack = true;
    lerpProgress = 0f;
  }

  private void PositionCameraTarget(float deltaTime)
  {
    var endPosition = sm.transform.position + new Vector3(0, sm.CameraTargetHeight);

    if (!isLerpingBack && sm.CameraTarget.transform.position != endPosition)
    {
      // POSIÇÂO INICIAL DE CameraTarget
      sm.CameraTarget.transform.position = endPosition;
      return;
    }
    endLerpPosition = endPosition;

    if (sm.CameraTarget.transform.position == endLerpPosition)
    {
      isLerpingBack = false;
      return;
    }

    lerpProgress += deltaTime * 2f;

    sm.CameraTarget.transform.position = Vector3.Lerp(startLerpPosition, endLerpPosition, lerpProgress);
  }
}