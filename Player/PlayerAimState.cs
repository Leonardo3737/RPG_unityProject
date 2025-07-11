using System.Runtime.InteropServices;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class PlayerAimState : PlayerBaseState
{
  private readonly int DrawArrowAnimation = Animator.StringToHash("Draw Arrow");
  private readonly int ShootAnimation = Animator.StringToHash("Shoot");
  private bool IsDrawArrowFinalized = false;
  private bool IsShootAnimationStart = false;
  private bool IsShooting = false;
  private float CenterOrbitRadius;
  private float SmoothCenterOrbitRadius;
  private float CurrentSmoothVelocity;
  private CinemachineOrbitalFollow Orbital;
  private Vector2 ScreenCenter;
  private Transform ArrowSpawn;
  private AudioSource AudioSource;
  private GameObject CurrentArrow;

  public PlayerAimState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.AIM) { }


  public override void Enter()
  {
    ScreenCenter = new(Screen.width / 2f, Screen.height / 2f);
    sm.IsAiming = true;
    sm.AimImage.enabled = true;

    PositionCameraTarget();

    ArrowSpawn = sm.CurrentEquipament.transform.Find("ArrowSpawn");
    AudioSource = sm.CurrentEquipament.GetComponentInChildren<AudioSource>();

    // SEMPRE HABILITAR CONTROLE DA CAMERA COM MOUSE
    EnableCameraInputController();

    sm.Animator.CrossFadeInFixedTime(DrawArrowAnimation, 0.1f, sm.EquippedLayer);
    sm.Animator.SetFloat(FreeLookSpeed, 0f);
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

    Shoot(targetPosition);
    CheckLineOfSight(targetPosition);

    var currentPosition = sm.transform.position;

    /* var cameraTargetPosition = sm.transform.position + new Vector3(0, sm.CameraTargetHeight);

    var cameraTargetFinalPosition = cameraTargetPosition + (sm.CameraTarget.transform.right * 0.3f) + (-sm.CameraTarget.transform.up * 0.3f);

    sm.CameraTarget.transform.position = Vector3.Lerp(sm.CameraTarget.transform.position, cameraTargetFinalPosition, deltaTime * 15f); */

    Vector3 lookDirection = targetPosition - currentPosition;
    lookDirection.y = 0;
    if (lookDirection != Vector3.zero)
    {
      Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
      //sm.transform.rotation = targetRotation;
      sm.transform.rotation = Quaternion.Slerp(sm.transform.rotation, targetRotation, 15f * deltaTime);
    }

    if (!IsDrawArrowFinalized && !IsShootAnimationStart)
    {
      var drawArrowNormalizedTime = GetNormalizedTime(sm.Animator, "drawArrow", sm.EquippedLayer);

      if (drawArrowNormalizedTime > 0.3f && CurrentArrow == null)
      {
        CurrentArrow = sm.SpawnArrow();
        AudioSource.PlayOneShot(sm.DrawArrowSound);
      }
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
    sm.DestroyObj(CurrentArrow);
    if (sm.AlternativeAimImage.enabled)
    {
      sm.AlternativeAimImage.enabled = false;
    }
    if (Orbital != null)
    {
      Orbital.Orbits.Center.Radius = CenterOrbitRadius;
    }
    if (sm.IsToShoot)
    {
      sm.IsToShoot = false;
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

    /* if (CurrentAnimationSmooth < 0.01f)
    {
    }
    else
    {
    } */
    if (CurrentAnimationSmooth < 0.01f)
    {
      if (sm.Animator.GetFloat(TriggerSpeedX) != 0)
      {
        sm.Animator.SetFloat(TriggerSpeedX, 0);
      }
      if (sm.Animator.GetFloat(TriggerSpeedZ) != 0)
      {
        sm.Animator.SetFloat(TriggerSpeedZ, 0);
      }
    }
    else
    {
      sm.Animator.SetFloat(TriggerSpeedX, velocityX, 0.1f, deltaTime);
      sm.Animator.SetFloat(TriggerSpeedZ, velocityZ, 0.1f, deltaTime);
    }
  }

  private void ApproachCamera()
  {
    if (Orbital != null && Orbital.Orbits.Center.Radius > 2f)
    {
      SmoothCenterOrbitRadius = Orbital.Orbits.Center.Radius;
      SmoothCenterOrbitRadius = Mathf.SmoothDamp(
          SmoothCenterOrbitRadius,
          2f,
          ref CurrentSmoothVelocity,
          0.1f
      );
      Orbital.Orbits.Center.Radius = SmoothCenterOrbitRadius;
    }
  }

  private void Shoot(Vector3 targetPosition)
  {
    if (!sm.IsToShoot) return;

    if (!IsShootAnimationStart)
    {
      AudioSource.PlayOneShot(sm.ShootArrowSound);
      sm.Animator.CrossFadeInFixedTime(ShootAnimation, 0.1f);
      IsShootAnimationStart = true;
      return;
    }
    var normalizedTime = GetNormalizedTime(sm.Animator, "shoot", sm.EquippedLayer);

    if (normalizedTime > 0.28f && !IsShooting)
    {
      sm.DestroyObj(CurrentArrow);
      CurrentArrow = null;
      sm.Shooting(targetPosition, ArrowSpawn);
      IsShooting = true;
    }
    if (normalizedTime > 1f)
    {
      IsShootAnimationStart = false;
      IsShooting = false;
      IsDrawArrowFinalized = false;
      sm.IsToShoot = false;

      sm.Animator.CrossFadeInFixedTime(DrawArrowAnimation, 0.1f, sm.EquippedLayer);
    }
  }

  private void CheckLineOfSight(Vector3 target)
  {
    var origin = ArrowSpawn.position;
    var direction = (target - origin).normalized;

    //Debug.DrawRay(origin, direction * 50f, Color.red, 1f);

    if (Physics.Raycast(origin, direction, out RaycastHit hit))
    {
      var distance = Vector3.Distance(hit.point, target);
      if (distance < 0.1f)
      {
        if (sm.AlternativeAimImage.enabled)
        {
          sm.AlternativeAimImage.enabled = false;
        }
        return;
      }

      if (hit.point != target && !sm.AlternativeAimImage.enabled)
      {
        var screenPosition = Camera.main.WorldToScreenPoint(hit.point);
        sm.AlternativeAimImage.enabled = true;

        sm.AlternativeAimImage.transform.position = screenPosition;
        return;
      }
      else if (hit.point == target && sm.AlternativeAimImage.enabled)
      {
        sm.AlternativeAimImage.enabled = false;
      }
      return;
    }
    if (sm.AlternativeAimImage.enabled)
    {
      sm.AlternativeAimImage.enabled = false;
    }
  }
}