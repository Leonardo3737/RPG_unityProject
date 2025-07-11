using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class PlayerBaseState : State
{
	protected PlayerStateMachine sm;

	protected static readonly int FreeLookBlendTree = Animator.StringToHash("FreeLookBlendTree");
	protected static readonly int TriggerBlendTree = Animator.StringToHash("TriggerBlendTree");
	protected static readonly int AimBlendTree = Animator.StringToHash("AimBlendTree");
	protected static readonly int FreeLookSpeed = Animator.StringToHash("FreeLookSpeed");
	protected static readonly int TriggerSpeedX = Animator.StringToHash("TriggerSpeedX");
	protected static readonly int TriggerSpeedZ = Animator.StringToHash("TriggerSpeedZ");

	protected float CurrentAnimationSmooth;
	protected float CurrentAnimationVelocity;

	private Vector2 CurrentMovementSmooth;
	private Vector2 CurrentMovementVelocity;

	private Vector3 previousMovement = Vector3.zero;


	protected bool WasOnTheMove;

	public PlayerBaseState(
			PlayerStateMachine stateMachine,
			StatesType stateType
			)
	{
		sm = stateMachine;
		StateType = stateType;
		WasOnTheMove = sm.InputHandler.InputMovement != Vector2.zero;
	}

	public void FreeLookMovement(float deltaTime)
	{
		// MOVEMENT
		var movement = CalculateMovement(!WasOnTheMove);
		movement = FreeLookMove(deltaTime, movement, !WasOnTheMove);

		// ROTATION
		FreeLookFaceMoveDirection(deltaTime, movement);
	}

	public void TriggerMovement(float deltaTime)
	{
		var target = sm.Targeter.CurrentTarget;

		if (target == null) return;

		var movement = CalculateMovement(!WasOnTheMove);

		TriggerMovement(deltaTime, movement);

		TriggerPositionCamera(deltaTime);

		var rotatioCameraIsCorrect = TriggerFaceMoveDirection(deltaTime, target.transform.position);

		var targetPosition = sm.Targeter.GetTargetPosition().Value;

		PositionCameraTargetInTarget(deltaTime, rotatioCameraIsCorrect);

		ToggleFocusIndicatorColor(targetPosition);
	}

	public bool FreeLookFaceMoveDirection(float deltaTime, Vector3 movement)
	{
		var lookRotation = movement == Vector3.zero ?
			Quaternion.LookRotation(sm.transform.forward) :
			Quaternion.LookRotation(movement);

		sm.transform.rotation = Quaternion.Slerp(sm.transform.rotation, lookRotation, sm.RotationSpeed * deltaTime);

		return Quaternion.Angle(sm.transform.rotation, lookRotation) < 0.1f;
	}

	public virtual Vector3 FreeLookMove(float deltaTime, Vector3 movement, bool applySmoothing = true)
	{
		movement = sm.Camera.transform.TransformDirection(movement);
		movement.y = 0;
		Move(deltaTime, movement);

		var inputMagnitude = sm.InputHandler.InputMovement.magnitude;

		if (!applySmoothing)
		{
			sm.Animator.SetFloat(FreeLookSpeed, inputMagnitude);
			return movement;
		}
		CurrentAnimationSmooth = Mathf.SmoothDamp(
				CurrentAnimationSmooth,
				sm.InputHandler.InputMovement == Vector2.zero ? 0f : inputMagnitude,
				ref CurrentAnimationVelocity,
				0.2f
		);

		if (Mathf.Abs(CurrentAnimationSmooth) < 0.01f)
		{
			if (sm.Animator.GetFloat(FreeLookSpeed) != 0)
			{
				sm.Animator.SetFloat(FreeLookSpeed, 0);
			}
		}
		else
		{
			sm.Animator.SetFloat(FreeLookSpeed, CurrentAnimationSmooth, 0.1f, deltaTime);
		}

		return movement;
	}

	protected void Move(float deltaTime, Vector3 movement)
	{
		sm.Controller.Move((movement + sm.ForceReceiver.Movement) * (sm.FreeLookSpeed * deltaTime));
	}

	public Vector3 CalculateMovement(bool applySmoothing = true)
	{
		if (!applySmoothing)
		{
			var input = sm.InputHandler.InputMovement;
			return new Vector3(input.x, 0, input.y);
		}

		CurrentMovementSmooth = Vector2.SmoothDamp(
				CurrentMovementSmooth,
				sm.InputHandler.InputMovement,
				ref CurrentMovementVelocity,
				0.2f
		);

		return new Vector3(CurrentMovementSmooth.x, 0, CurrentMovementSmooth.y);
	}

	protected void EnableCameraInputController()
	{
		if (sm.FreeLookCamera.TryGetComponent(out CinemachineInputAxisController inputController) && !inputController.enabled)
		{
			inputController.enabled = true;
		}
	}

	protected float GetNormalizedTime(Animator animator, string tag, int layerIndex = -1)
	{
		if (layerIndex == -1)
		{
			layerIndex = sm.CurrentLayer;
		}

		var currentState = animator.GetCurrentAnimatorStateInfo(layerIndex);
		var nextState = animator.GetNextAnimatorStateInfo(layerIndex);

		if (animator.IsInTransition(layerIndex) && nextState.IsTag(tag))
		{
			return nextState.normalizedTime;
		}
		if (currentState.IsTag(tag))
		{
			return currentState.normalizedTime;
		}
		return 0f;
	}

	public Vector3 TriggerMovement(float deltaTime, Vector3 movement, bool applySmoothing = true)
	{
		var target = sm.Targeter.CurrentTarget;
		if (target == null) return Vector3.zero;

		// Direção para o alvo (frente do player em relação ao inimigo)
		Vector3 toTarget = target.transform.position - sm.transform.position;
		toTarget.y = 0;
		toTarget.Normalize();

		// Direção lateral (strafe em torno do alvo)
		Vector3 strafeDir = Quaternion.AngleAxis(90f, Vector3.up) * toTarget;

		// Direção desejada com base no input
		Vector3 targetMovement = (toTarget * sm.InputHandler.InputMovement.y) +
														 (strafeDir * sm.InputHandler.InputMovement.x);

		// Suaviza entre o movimento anterior e o atual
		Vector3 smoothMovement = Vector3.Lerp(previousMovement, targetMovement.normalized, deltaTime * 4f);

		// Salva para o próximo frame
		previousMovement = smoothMovement;

		// Aplica o movimento
		sm.Controller.Move((smoothMovement + sm.ForceReceiver.Movement) * (sm.FreeLookSpeed * deltaTime));

		// Animação
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

		return movement;
	}

	public void TriggerPositionCamera(float deltaTime)
	{
		if (!sm.FreeLookCamera.TryGetComponent(out CinemachineOrbitalFollow orbitalFollow)) return;
		var target = sm.Targeter.CurrentTarget;

		float angleTarget;

		if (target != null && sm.IsTriggered)
		{
			Vector3 direction = target.transform.position - sm.transform.position;
			direction.y = 0;

			Quaternion targetRotation = Quaternion.LookRotation(direction);
			angleTarget = targetRotation.eulerAngles.y;
		}
		else
		{
			angleTarget = sm.transform.eulerAngles.y;
		}

		// Suaviza a rotação da câmera ao redor do alvo
		orbitalFollow.HorizontalAxis.Value = Mathf.LerpAngle(
				orbitalFollow.HorizontalAxis.Value,
				angleTarget,
				deltaTime * 7f
		);

		// Suaviza altura da câmera (opcional)
		orbitalFollow.VerticalAxis.Value = Mathf.LerpAngle(
				orbitalFollow.VerticalAxis.Value,
				25f,
				deltaTime * 7f
		);

	}

	public void ToggleFocusIndicatorColor(Vector3 targetPosition)
	{
		var target = sm.Targeter.CurrentTarget;
		if (target == null) return;

		var distance = Vector3.Distance(sm.transform.position, targetPosition);
		var image = target.FocusIndicatorImage;

		if (distance < sm.MaxAttackDistanceOnTrigger)
		{
			image.color = Colors.TransparentGreen;
		}
		else
		{
			image.color = Colors.TransparentRed;
		}
	}

	public bool TriggerFaceMoveDirection(float deltaTime, Vector3 targetPosition)
	{
		Vector3 directionToTarget = targetPosition - sm.transform.position;
		directionToTarget.y = 0;

		if (directionToTarget == Vector3.zero) return true;

		// Reduzimos a direção "para trás"
		float offsetFactor = 0.3f;
		Vector3 offsetBack = -sm.transform.forward * offsetFactor;

		// Aplica o leve recuo ao vetor final
		Vector3 adjustedDirection = directionToTarget + offsetBack;
		adjustedDirection.Normalize();

		Quaternion targetRotation = Quaternion.LookRotation(adjustedDirection);
		sm.transform.rotation = Quaternion.Lerp(sm.transform.rotation, targetRotation, sm.RotationSpeed * deltaTime);

		return Quaternion.Angle(sm.transform.rotation, targetRotation) < 0.1f;
	}

	public bool CheckTarget()
	{
		if (sm.Targeter.SelectTarget())
		{
			return true;
		}

		sm.IsTriggered = false;
		sm.ChangeState(new PlayerFreeLookState(sm));

		return false;
	}

	public void PositionCameraTargetInTarget(float deltaTime, bool ApplySmoothing)
	{
		if (sm.IsTriggered && sm.Targeter.CurrentTarget != null)
		{
			var targetPosition = sm.Targeter.GetTargetPosition().Value;
			if (sm.JustRolled)
			{
				sm.CameraTarget.transform.position = targetPosition;
				if (ApplySmoothing)
				{
					sm.JustRolled = false;
				}
			}
			else
			{
				var currentPosition = sm.CameraTarget.transform.position;

				sm.CameraTarget.transform.position = ApplySmoothing ? Vector3.Lerp(currentPosition, targetPosition, deltaTime * 3f) : targetPosition;
			}
		}
	}

	protected Vector3 FaceInputDirection(bool ApplySmoothing = false, float deltaTime = 0.1f)
	{
		Vector2 input = sm.InputHandler.InputMovement;
		if (input == Vector2.zero) return Vector3.zero;

		// Direção no espaço do mundo, baseado na câmera
		Vector3 direction = new Vector3(input.x, 0, input.y);
		direction = sm.Camera.transform.TransformDirection(direction);
		direction.y = 0;

		if (direction.sqrMagnitude < 0.01f) return Vector3.zero;

		if (ApplySmoothing)
		{
			var newRotation = Quaternion.LookRotation(direction);
			sm.transform.rotation = Quaternion.Slerp(sm.transform.rotation, newRotation, sm.RotationSpeed * deltaTime);
		}
		else
		{
			sm.transform.rotation = Quaternion.LookRotation(direction);
		}

		return direction;
	}
	protected void PositionCameraTarget()
	{
		var endPosition = sm.transform.position + new Vector3(0, sm.CameraTargetHeight);

		if (sm.CameraTarget.transform.position != endPosition)
		{
			sm.CameraTarget.transform.position = endPosition;
			return;
		}
	}
}
