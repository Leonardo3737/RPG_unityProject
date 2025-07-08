using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateMachine : StateMachine
{
	[Header("Scripts")]
	[field: SerializeField]
	public InputHandler InputHandler { get; private set; }

	[Header("variables")]

	[field: SerializeField]
	public float CameraTargetHeight { get; private set; } = 1.75f;

	[field: SerializeField]
	public float MaxAttackDistanceOnTrigger { get; set; } = 6f;

	[field: SerializeField]
	public float MaxAttackDistance { get; set; } = 3f;

	[Header("Scene")]
	public TextMeshProUGUI UIText;
	public GameObject BowPrefab;
	public GameObject ArrowPrefab;
	public Transform BowHolder;

	[field: SerializeField]
	public GameObject CameraTarget { get; private set; }

	[field: SerializeField]
	public GameObject MeshRoot { get; private set; }

	[field: SerializeField]
	public Camera Camera { get; private set; }

	[field: SerializeField]
	public CinemachineCamera FreeLookCamera { get; private set; }

	[field: SerializeField]
	public PlayerTargeter Targeter { get; private set; }

	[field: SerializeField]
	public Image AimImage { get; set; }

	[Header("Movement Speed")]

	[field: SerializeField]
	public float RotationSpeed { get; private set; } = 15f;

	[field: SerializeField]
	public float TriggerSpeed { get; private set; } = 3.5f;

	[field: SerializeField]
	public float RollSpeed { get; private set; } = 6f;

	[field: SerializeField]
	public float FreeLookSpeed { get; private set; } = 5f;

	[field: SerializeField]
	public float AimSpeed { get; private set; } = 3f;


	[Header("Animator Controllers")]

	[field: SerializeField]
	public Animator Animator { get; private set; }

	public event Action OnCancelAttackEvent;

	public bool IsTriggered { get; set; } = false;
	public bool JustRolled { get; set; }
	public bool IsChangingTarget { get; set; }
	public bool CancelAttack { get; set; } = false;
	public bool IsAiming { get; set; } = false;
	public bool IsShooting { get; set; } = false;
	public float RegularControllerHeigth { get; set; } = 1.5f;
	public float CrouchedControllerHeigth { get; set; } = 1.2f;
	public int AttackIndex { get; set; } = 0;
	public int UnequippedLayer { get; set; } = 1;
	public int EquippedLayer { get; set; } = 2;
	public int CurrentLayer { get; set; }

	public Modes CurrentMode = Modes.UNARMED;
	public GameObject CurrentEquipament;

	void Start()
	{
		AimImage.enabled = false;
		CurrentLayer = UnequippedLayer;

		ChangeState(new PlayerFreeLookState(this));

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public override void ChangeState(State newState)
	{
		UIText.text = newState.StateType.ToString();
		base.ChangeState(newState);
	}
	public override void Update()
	{
		if (Targeter.CurrentTarget != null && Targeter.CurrentTarget.IsBeingFocused != IsTriggered)
		{
			Targeter.CurrentTarget.IsBeingFocused = IsTriggered;
		}
		base.Update();
	}

	private void OnEnable()
	{
		InputHandler.OnMainAttackEvent += HandleMainAttack;
		InputHandler.OnAlternativeAttackEvent += HandleAlternativeAttack;
		InputHandler.OnTriggerEvent += HandleTrigger;
		InputHandler.OnRollEvent += HandleRoll;
		InputHandler.OnChangeTriggerEvent += HandleChangeTrigger;
		InputHandler.OnToggleModesEvent += HandleToggleModes;
	}
	private void OnDisable()
	{
		InputHandler.OnMainAttackEvent -= HandleMainAttack;
		InputHandler.OnAlternativeAttackEvent -= HandleAlternativeAttack;
		InputHandler.OnTriggerEvent -= HandleTrigger;
		InputHandler.OnRollEvent -= HandleRoll;
		InputHandler.OnChangeTriggerEvent -= HandleChangeTrigger;
		InputHandler.OnToggleModesEvent -= HandleToggleModes;
	}

	public void HandleMainAttack()
	{
		if (!((PlayerBaseState)currentState).CanPerformAction()) return;
		switch (CurrentMode)
		{
			case Modes.UNARMED:
				ChangeState(new PlayerAttackState(this, AttackTypes.MainAttack));
				break;
			case Modes.EQUIPPED:
				if (IsShooting) break;
				IsShooting = true;
				break;
		}
	}

	public void HandleAlternativeAttack()
	{
		if (!((PlayerBaseState)currentState).CanPerformAction()) return;

		switch (CurrentMode)
		{
			case Modes.UNARMED:
				ChangeState(new PlayerAttackState(this, AttackTypes.AlternativeAttack));
				break;
			case Modes.EQUIPPED:
				TogglePlayerAim();
				break;
		}
	}

	public void TogglePlayerAim()
	{
		if (IsAiming)
		{
			ChangeState(new PlayerFreeLookState(this));
		}
		else
		{
			ChangeState(new PlayerAimState(this));
		}
	}

	public void HandleTrigger()
	{
		if (
				!((PlayerBaseState)currentState).CanPerformAction()
				|| !Targeter.SelectTarget()
				) return;

		IsTriggered = !IsTriggered;
		if (IsTriggered)
		{
			ChangeState(new PlayerTriggerState(this));
		}
		else
		{
			ChangeState(new PlayerFreeLookState(this));
		}
	}

	public void HandleRoll()
	{
		CancelAttack = true;
		if (!((PlayerBaseState)currentState).CanPerformAction()) return;
		ChangeState(new PlayerRollState(this));
	}

	public void HandleToggleModes()
	{
		if (!((PlayerBaseState)currentState).CanPerformAction()) return;
		ChangeState(new PlayerToggleModesState(this));
	}

	public void HandleChangeTrigger()
	{
		IsChangingTarget = true;
		Targeter.ChangeTarget();
		IsChangingTarget = false;
	}

	public void OnCancelAttack()
	{
		OnCancelAttackEvent.Invoke();
	}

	public void ToggleMode()
	{
		if (CurrentMode == Modes.UNARMED)
		{
			CurrentEquipament = Instantiate(BowPrefab, BowHolder);
			CurrentEquipament.transform.SetLocalPositionAndRotation(
					new Vector3(-0.024f, 0.785f, -0.384f),
					Quaternion.Euler(-7.49f, 90.626f, 85.21f)
					);
			CurrentMode = Modes.EQUIPPED;
		}
		else if (CurrentEquipament != null)
		{
			Destroy(CurrentEquipament);
			CurrentEquipament = null;
			CurrentMode = Modes.UNARMED;
		}
	}

	public void ToggleAnimatorController()
	{
		StopAllCoroutines(); // Interrompe qualquer transição anterior
		if (CurrentMode == Modes.EQUIPPED)
		{
			StartCoroutine(SmoothLayerWeight(UnequippedLayer, 1f, 0.3f)); // fade-out
			StartCoroutine(SmoothLayerWeight(EquippedLayer, 1f, 0.3f));   // fade-in
			CurrentLayer = EquippedLayer;
		}
		else
		{
			StartCoroutine(SmoothLayerWeight(EquippedLayer, 0f, 0.3f));   // fade-out
			StartCoroutine(SmoothLayerWeight(UnequippedLayer, 1f, 0.3f)); // fade-in
			CurrentLayer = UnequippedLayer;
		}
	}

	private IEnumerator SmoothLayerWeight(int layerIndex, float targetWeight, float duration)
	{
		float time = 0f;
		float startWeight = Animator.GetLayerWeight(layerIndex);

		while (time < duration)
		{
			time += Time.deltaTime;
			float newWeight = Mathf.Lerp(startWeight, targetWeight, time / duration);
			Animator.SetLayerWeight(layerIndex, newWeight);
			yield return null;
		}

		// Garante o valor final exato
		Animator.SetLayerWeight(layerIndex, targetWeight);
	}

	public void Shooting(Vector3 target)
	{
		var Arrow = Instantiate(ArrowPrefab);
		if (Arrow.TryGetComponent(out Arrow arrow))
		{
			arrow.SetTarget(target);
		}
		var spawn = CurrentEquipament.transform.Find("ArrowSpawn");
		if (spawn == null)
		{
			Destroy(Arrow);
			return;
		}

		Vector3 direction = (target - spawn.position).normalized;
		Quaternion rotation = Quaternion.LookRotation(direction);

		Arrow.transform.SetPositionAndRotation(spawn.position, rotation);
	}

}
