using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
  [SerializeField]
  private MonoBehaviour animationEventSourceMono;

  private IAttackAnimationEvents AnimationEvents;

  [SerializeField]
  private bool CanDealDamage;
  private List<GameObject> HasDealtDamage;

  [SerializeField]
  private float WeaponLength;

  [SerializeField]
  private float weaponAngleDegrees_X;

  [SerializeField]
  private float weaponAngleDegrees_Y;

  [SerializeField]
  private float weaponAngleDegrees_Z;

  [SerializeField]
  private float WeaponPosition_X;

  [SerializeField]
  private float WeaponPosition_Y;

  [SerializeField]
  private float WeaponPosition_Z;

  [SerializeField]
  private int WeaponDamage = 10;

  [SerializeField]
  private int Layer;

  [SerializeField]
  private List<DamageAction> Actions;

  [SerializeField]
  private string AnimationName;

  [SerializeField]
  private int ActionIndex;

  public void Start()
  {
    CanDealDamage = false;
    HasDealtDamage = new();

  }

  public void OnEnable()
  {
    AnimationEvents = animationEventSourceMono as IAttackAnimationEvents;

    AnimationEvents.OnStartAttackEvent += StartDealDamage;
    AnimationEvents.OnEndAttackEvent += EndDealDamage;
    AnimationEvents.OnCancelAttackEvent += EndDealDamage;
  }

  public void OnDisable()
  {
    AnimationEvents.OnStartAttackEvent -= StartDealDamage;
    AnimationEvents.OnEndAttackEvent -= EndDealDamage;
    AnimationEvents.OnCancelAttackEvent -= EndDealDamage;
  }

  public void Update()
  {
    if (CanDealDamage)
    {
      Vector3 origin = transform.position + (transform.forward * WeaponPosition_Z) + (transform.up * WeaponPosition_Y) + (transform.right * WeaponPosition_X); ;
      Vector3 direction = transform.rotation * Quaternion.Euler(weaponAngleDegrees_X, weaponAngleDegrees_Y, weaponAngleDegrees_Z) * Vector3.up;

      var layerMask = 1 << Layer;

      if (Physics.Raycast(origin, direction, out RaycastHit hit, WeaponLength, layerMask))
      {
        if (hit.transform.TryGetComponent(out StateMachine sm) && !HasDealtDamage.Contains(hit.transform.gameObject))
        {

          if (sm.IsDefending) return;

          if (!AnimationEvents.GetEffectiveAttack())
          {
            AnimationEvents.SetEffectiveAttack(true);
          }

          HasDealtDamage.Add(hit.transform.gameObject);

          Vector3 directionToOrigin = transform.root.transform.position - hit.transform.root.position;

          directionToOrigin.y = 0; // ignora diferença de altura

          hit.transform.rotation = Quaternion.LookRotation(directionToOrigin);

          var Action = ActionIndex >= Actions.Count ? null : Actions[ActionIndex];

          sm.OnDamage(WeaponDamage, AnimationName, Action);
        }
      }
    }
  }

  public void Exit()
  {

  }

  public void StartDealDamage(string animationName, int actionIndex)
  {
    AnimationName = animationName;
    ActionIndex = actionIndex;
    if (!CanDealDamage)
    {
      CanDealDamage = true;
    }
    HasDealtDamage.Clear();
  }

  public void EndDealDamage()
  {
    if (CanDealDamage)
    {
      CanDealDamage = false;
    }
    if (AnimationEvents.GetEffectiveAttack())
    {
      AnimationEvents.SetEffectiveAttack(false);
    }
  }

  private void OnDrawGizmos()
  {
    Vector3 origin = transform.position + (transform.forward * WeaponPosition_Z) + (transform.up * WeaponPosition_Y) + (transform.right * WeaponPosition_X);
    Vector3 direction = transform.rotation * Quaternion.Euler(weaponAngleDegrees_X, weaponAngleDegrees_Y, weaponAngleDegrees_Z) * Vector3.up;
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(origin, transform.position + direction * WeaponLength);
  }

}




