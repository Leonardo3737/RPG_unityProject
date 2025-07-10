using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class EnemyStateMachine : StateMachine
{

  [field: SerializeField]
  public Animator Animator { get; private set; }

  [field: SerializeField]
  public EnemyTargeter Targeter { get; private set; }

  [field: SerializeField]
  public GameObject MeshRoot { get; private set; }

  [field: SerializeField]
  public float PatrolSpeed { get; private set; } = 2f;

  [field: SerializeField]
  public float PursuitSpeed { get; private set; } = 4.8f;

  [field: SerializeField]
  public float RotationSpeed { get; private set; } = 15f;

  [field: SerializeField]
  public float RollSpeed { get; private set; } = 6f;

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

  public int AttackIndex = 0;
  public bool IsBeingFocused = false;
  public bool IsInvestigatingSound = false;

  public bool IsDie { get; set; } = false;

  public Vector3? LastSoundOrigin;

  public event Action<EnemyStateMachine> OnDieEvent;

  void Start()
  {
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
    if (IsInvestigatingSound && HasReachedDestination())
    {
      Debug.Log("terminou sua investigação");
      IsInvestigatingSound = false;
      LastSoundOrigin = null;
    }

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
  }

  public void HandleTriggerEnter()
  {
    if (Targeter.SelectTarget() && Targeter.HasLineOfSight() && IsPatrol)
    {
      ChangeState(new EnemyPursuitState(this));
    }
  }

  public void OnDamage(int damage, string DamageAnimationName, DamageAction Action)
  {
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
    if (currentState.StateType == StatesType.PURSUIT) return;

    var aux = true;

    if (LastSoundOrigin != null)
    {
      var distance = Vector3.Distance(origin, LastSoundOrigin.Value);

      aux = distance < 2f;
    }

    if (IsInvestigatingSound && aux) return;

    IsInvestigatingSound = true;

    Debug.Log("escutou");

    Vector3 directionToOrigin = origin - transform.position;
    directionToOrigin.y = 0;

    NavMeshAgent.destination = origin - (directionToOrigin * 0.3f);
    NavMeshAgent.speed = PursuitSpeed;



    /* Vector3 directionToOrigin = origin - transform.position;

    directionToOrigin.y = 0; // ignora diferença de altura

    transform.rotation = Quaternion.LookRotation(directionToOrigin); */

  }

  public bool HasReachedDestination()
  {
    return !NavMeshAgent.pathPending &&
           NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance &&
           (!NavMeshAgent.hasPath || NavMeshAgent.velocity.sqrMagnitude < 0.01f);
  }

}