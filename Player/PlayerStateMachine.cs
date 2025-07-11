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
	public Transform ArrowHolder;

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

	[field: SerializeField]
	public Image AlternativeAimImage { get; set; }

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


	[Header("Sounds")]

	[field: SerializeField]
	public AudioSource FootStepAudioSource { get; private set; }

	[field: SerializeField]
	public AudioSource VoiceAudioSource { get; private set; }

	[field: SerializeField]
	public AudioSource MiddleAudioSource { get; private set; }

	[field: SerializeField]
	public AudioClip[] FootStepSounds { get; private set; }

	[field: SerializeField]
	public AudioClip[] JumpStartSounds { get; private set; }

	[field: SerializeField]
	public AudioClip[] JumpEndSounds { get; private set; }

	[field: SerializeField]
	public AudioClip[] RollSounds { get; private set; }

	[field: SerializeField]
	public AudioClip[] AttackSounds { get; private set; }

	[field: SerializeField]
	public AudioClip DrawArrowSound { get; private set; }

	[field: SerializeField]
	public AudioClip ShootArrowSound { get; private set; }

	[field: SerializeField]
	public AudioClip ToggleModeSound { get; private set; }

	[Header("Animator Controllers")]

	[field: SerializeField]
	public Animator Animator { get; private set; }


	public event Action OnCancelAttackEvent;

	public bool IsTriggered { get; set; } = false;
	public bool IsJumping { get; set; } = false;
	public bool JustRolled { get; set; }
	public bool IsChangingTarget { get; set; }
	public bool CancelAttack { get; set; } = false;
	public bool IsAiming { get; set; } = false;
	public bool IsToShoot { get; set; } = false;
	public float RegularControllerHeigth { get; set; } = 1.5f;
	public float CrouchedControllerHeigth { get; set; } = 1f;
	public int AttackIndex { get; set; } = 0;
	public int UnequippedLayer { get; set; } = 1;
	public int EquippedLayer { get; set; } = 2;
	public int CurrentLayer { get; set; }

	public Modes CurrentMode = Modes.UNARMED;
	public GameObject CurrentEquipament;

	void Start()
	{
		AimImage.enabled = false;
		AlternativeAimImage.enabled = false;
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
		InputHandler.OnJumpEvent += HandleJump;
	}
	private void OnDisable()
	{
		InputHandler.OnMainAttackEvent -= HandleMainAttack;
		InputHandler.OnAlternativeAttackEvent -= HandleAlternativeAttack;
		InputHandler.OnTriggerEvent -= HandleTrigger;
		InputHandler.OnRollEvent -= HandleRoll;
		InputHandler.OnChangeTriggerEvent -= HandleChangeTrigger;
		InputHandler.OnToggleModesEvent -= HandleToggleModes;
		InputHandler.OnJumpEvent -= HandleJump;
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
				if (IsToShoot || currentState.StateType != StatesType.AIM) break;
				IsToShoot = true;
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

	public void HandleJump()
	{
		if (!((PlayerBaseState)currentState).CanPerformAction() || IsJumping) return;
		IsJumping = true;
		ChangeState(new PlayerJumpState(this));
	}

	public void OnCancelAttack()
	{
		OnCancelAttackEvent.Invoke();
	}

	public void ToggleMode()
	{
		MiddleAudioSource.PlayOneShot(ToggleModeSound);
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
			StartCoroutine(SmoothLayerWeight(UnequippedLayer, 0f, 0.3f)); // fade-out
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

	public GameObject SpawnArrow()
	{
		var arrow = Instantiate(ArrowPrefab, ArrowHolder);
		arrow.transform.position = new Vector3(0.027f, 0.404f, 0.047f);
		arrow.transform.SetLocalPositionAndRotation(
			new Vector3(0.027f, 0.404f, 0.047f),
			Quaternion.Euler(-82.518f, 29.145f, -46.026f)
		);

		/* if (arrow.TryGetComponent(out arrow arrow))
		{

		} */

		return arrow;
	}

	public void DestroyObj(GameObject obj)
	{
		Destroy(obj);
	}

	public void Shooting(Vector3 target, Transform spawn)
	{
		var Arrow = Instantiate(ArrowPrefab);

		if (spawn == null)
		{
			Destroy(Arrow);
			return;
		}

		Arrow.transform.position = spawn.position;

		if (Arrow.TryGetComponent(out Arrow arrow))
		{
			arrow.SetTarget(target);
		}

		Vector3 direction = (target - spawn.position).normalized;
		Quaternion rotation = Quaternion.LookRotation(direction);

		//Arrow.transform.SetPositionAndRotation(spawn.position, rotation);
	}

	public void MakeSound(AudioSource source, AudioClip sound)
	{
		source.PlayOneShot(sound);

		var radius = source.maxDistance;
		var position = transform.position;

		Collider[] listeners = Physics.OverlapSphere(position, radius, LayerMask.GetMask("Enemy"));
		
    foreach (var col in listeners)
		{
			if (!col.TryGetComponent(out EnemyStateMachine enemyStateMachine)) continue;
      enemyStateMachine.OnHeardSound(position);
		}
	}

}
