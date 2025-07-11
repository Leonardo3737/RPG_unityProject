using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class EnemyStateMachine : StateMachine
{

  public TextMeshProUGUI UIText;

  [field: SerializeField]
  public Animator Animator { get; private set; }

  [field: SerializeField]
  public EnemyTargeter Targeter { get; private set; }

  [field: SerializeField]
  public GameObject MeshRoot { get; private set; }

  [field: SerializeField]
  public float PatrolSpeed { get; private set; } = 2f;

  [field: SerializeField]
  public float ChaseSpeed { get; private set; } = 4.8f;

  [field: SerializeField]
  public float RotationSpeed { get; private set; } = 15f;

  [field: SerializeField]
  public float RollSpeed { get; private set; } = 6f;

  [field: SerializeField]
  public float MaxAttackDistance { get; private set; } = 1.2f;

  [field: SerializeField]
  public NavMeshAgent NavMeshAgent { get; private set; }

  public bool IsTriggered = false;

  [field: SerializeField]
  public bool IsPatrol { get; set; } = true;

  [field: SerializeField]
  public float MaxHealth { get; set; } = 100;

  [field: SerializeField]
  public float CurrentHealth { get; set; } = 100;

  [field: SerializeField]
  public Image HealthImage { get; set; }

  [field: SerializeField]
  public Image FocusIndicatorImage { get; set; }

  [field: SerializeField]
  public Canvas HealthCanvas { get; set; }

  [field: SerializeField]
  public AudioSource AudioSource { get; private set; }

  [field: SerializeField]
  public AudioClip[] DamageSounds { get; private set; }

  public int AttackIndex { get; set; } = 0;
  public bool IsBeingFocused { get; set; } = false;
  public bool IsInvestigatingSound { get; set; } = false;
  public bool CancelAttack { get; set; } = false;
  public bool JustChased { get; set; } = false;

  public bool IsDie { get; set; } = false;

  public Vector3? LastSoundOrigin { get; set; }

  public event Action<EnemyStateMachine> OnDieEvent;

  public override void ChangeState(State newState)
  {
    UIText.text = newState.StateType.ToString();
    base.ChangeState(newState);
  }

  void Start()
  {
    NavMeshAgent.updateRotation = false;
    ChangeState(new EnemyPatrolState(this));
  }

  private void OnEnable()
  {
    Targeter.OnTriggerEnterEvent += HandleTriggerEnter;
  }
  private void OnDisable()
  {
    Targeter.OnTriggerEnterEvent -= HandleTriggerEnter;
  }

  public override void Update()
  {

    if (HealthCanvas != null && FocusIndicatorImage != null && FocusIndicatorImage.enabled != IsBeingFocused)
    {
      FocusIndicatorImage.enabled = IsBeingFocused;
    }

    if (CurrentHealth <= 0 && currentState.StateType != StatesType.DEATH)
    {
      ChangeState(new EnemyDeathState(this));
    }
    if (!IsDie && HealthCanvas != null)
    {
      float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
      HealthCanvas.enabled = distance > 3f;
      HealthCanvas.transform.LookAt(HealthCanvas.transform.position + Camera.main.transform.forward);
    }
    base.Update();
    FaceMoveDirection();
  }

  public void HandleTriggerEnter()
  {
    if (Targeter.SelectTarget() && Targeter.HasLineOfSight() && IsPatrol)
    {
      ChangeState(new EnemyChaseState(this));
    }
  }

  public void OnDamage(int damage, string DamageAnimationName, DamageAction Action)
  {
    CancelAttack = true;
    
    if (!currentState.CanPerformAction()) return;

    ChangeState(
      new EnemyDamageState(
          this,
          damage,
          DamageAnimationName,
          Action
        )
      );
  }

  public void SetIsDie()
  {
    OnDieEvent?.Invoke(this);
    IsDie = true;
    DestroyCanvas();
  }

  public void Die()
  {
    Destroy(gameObject, 3f);
  }

  private void DestroyCanvas()
  {
    if (HealthCanvas == null) return;

    var timing = 0.5f;

    if (HealthCanvas.TryGetComponent<CanvasScaler>(out var scaler)) Destroy(scaler, timing);

    if (HealthCanvas.TryGetComponent<GraphicRaycaster>(out var raycaster)) Destroy(raycaster, timing);

    Destroy(HealthCanvas, timing);
  }

  public void OnHeardSound(Vector3 origin)
  {
    if (
      !currentState.CanPerformAction() ||
      currentState.StateType != StatesType.PATROL
      ) return;

    ChangeState(new EnemyInvestigateState(this, origin));
  }

  

  public virtual bool FaceMoveDirection()
  {
    Vector3 velocity = NavMeshAgent.velocity;

    if (velocity.sqrMagnitude < 0.0001f && currentState.StateType != StatesType.ATTACK)
        return true;

    Vector3 direction;

    if (currentState.StateType == StatesType.ATTACK)
    {
      direction = (Targeter.CurrentTarget.transform.position - transform.position).normalized;
    }
    else
    {
      direction = velocity.normalized;
    }

    Quaternion lookRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        lookRotation,
        RotationSpeed * Time.deltaTime
    );

    return Quaternion.Angle(transform.rotation, lookRotation) < 0.1f;
  }

}