using System.Runtime.InteropServices;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class PlayerAimState : PlayerBaseState
{
  private readonly int DrawArrowAnimation = Animator.StringToHash("Draw Arrow");
  private bool IsDrawArrowFinalized = false;

  private float CenterOrbitRadius;
  private float SmoothCenterOrbitRadius;
  private float CurrentSmoothVelocity;
  private CinemachineOrbitalFollow Orbital;
  private Vector2 ScreenCenter;
  private Vector3 PreviousTargetPosition;
  public PlayerAimState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.AIM) { }


  public override void Enter()
  {
    ScreenCenter = new(Screen.width / 2f, Screen.height / 2f);
    sm.IsAiming = true;
    sm.AimImage.enabled = true;
    sm.Animator.CrossFadeInFixedTime(DrawArrowAnimation, 0.1f, sm.EquippedLayer);
    if (sm.FreeLookCamera.TryGetComponent(out Orbital))
    {
      CenterOrbitRadius = Orbital.Orbits.Center.Radius;
    }
  }

  public override void Update(float deltaTime)
  {
    ApproachCamera();

    var ray = sm.Camera.ScreenPointToRay(ScreenCenter);

    Vector3 targetPosition;

    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    {
      targetPosition = hit.point;
    }
    else
    {
      targetPosition = ray.origin + ray.direction * 100f;
    }

    if (sm.IsShooting)
    {
      sm.Shooting(targetPosition);
      sm.IsShooting = false;
    }

    PreviousTargetPosition = targetPosition;


    var currentPosition = sm.transform.position;

    var cameraTargetPosition = sm.transform.position + new Vector3(0, sm.CameraTargetHeight);

    var cameraTargetFinalPosition = cameraTargetPosition + (sm.CameraTarget.transform.right * 0.7f) + (-sm.CameraTarget.transform.up * 0.3f);

    sm.CameraTarget.transform.position = Vector3.Lerp(sm.CameraTarget.transform.position, cameraTargetFinalPosition, deltaTime * 3f);

    Vector3 lookDirection = targetPosition - currentPosition;
    lookDirection.y = 0;
    if (lookDirection != Vector3.zero)
    {
      Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
      sm.transform.rotation = targetRotation;
    }

    if (!IsDrawArrowFinalized)
    {
      var drawArrowNormalizedTime = GetNormalizedTime(sm.Animator, "drawArrow", sm.EquippedLayer);

      if (drawArrowNormalizedTime < 0.85f)
      {
        return;
      }
      sm.Animator.CrossFadeInFixedTime(AimBlendTree, 0.1f, sm.EquippedLayer);
      IsDrawArrowFinalized = true;
    }

    var movement = CalculateMovement();
    Movement(deltaTime, movement);
  }

  public override void Exit()
  {
    if (Orbital != null)
    {
      Orbital.Orbits.Center.Radius = CenterOrbitRadius;
    }
    sm.AimImage.enabled = false;
    sm.IsAiming = false;
  }


  public override bool CanPerformAction()
  {
    return true;
  }

  private void Movement(float deltaTime, Vector3 movement)
  {
    movement = sm.Camera.transform.TransformDirection(movement);
    movement.y = 0;

    sm.Controller.Move((movement + sm.ForceReceiver.Movement) * (sm.AimSpeed * deltaTime));

    CurrentAnimationSmooth = Mathf.SmoothDamp(
        CurrentAnimationSmooth,
        sm.InputHandler.InputMovement == Vector2.zero ? 0f : sm.InputHandler.InputMovement.magnitude,
        ref CurrentAnimationVelocity,
        0.2f
    );

    var input = sm.InputHandler.InputMovement.normalized;

    float velocityX = CurrentAnimationSmooth * input.x;
    float velocityZ = CurrentAnimationSmooth * input.y;

    sm.Animator.SetFloat(TriggerSpeedX, velocityX, 0.1f, deltaTime);
    sm.Animator.SetFloat(TriggerSpeedZ, velocityZ, 0.1f, deltaTime);
  }

  private void ApproachCamera()
  {
    if (Orbital != null && Orbital.Orbits.Center.Radius > 3f)
    {
      SmoothCenterOrbitRadius = Orbital.Orbits.Center.Radius;
      SmoothCenterOrbitRadius = Mathf.SmoothDamp(
          SmoothCenterOrbitRadius,
          3f,
          ref CurrentSmoothVelocity,
          0.2f
      );
      Orbital.Orbits.Center.Radius = SmoothCenterOrbitRadius;
    }
  }

}