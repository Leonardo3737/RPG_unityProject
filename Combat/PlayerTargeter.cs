using UnityEngine;

public class PlayerTargeter : Targeter<EnemyStateMachine>
{

  public override void OnTriggerEnter(Collider other)
  {
    if (!other.TryGetComponent(out EnemyStateMachine target)) return;

    _targets.Add(target);

    target.OnDieEvent += RemoveTarget;
  }


  public override void OnTriggerExit(Collider other)
  {
    if (!other.TryGetComponent(out EnemyStateMachine target)) return;
    target.OnDieEvent -= RemoveTarget;
    RemoveTarget(target);
  }



  public override bool ChangeTarget()
  {
    if (CurrentTarget != null)
    {
      CurrentTarget.IsBeingFocused = false;
    }
    return base.ChangeTarget();
  }

  public override bool HasLineOfSight()
  {
    var targetPosition = GetTargetPosition();

    targetPosition -= Vector3.up * 0.3f;
    var origin = transform.position;

    var direction = (targetPosition - origin)?.normalized;

    if (direction == null)
    {
      return false;
    }

    var viewDistance = 0f;
    if (TryGetComponent(out CapsuleCollider capsuleCollider))
    {
      viewDistance = capsuleCollider.radius;
    }

    //Debug.DrawRay(origin, (Vector3)direction * viewDistance, Color.red, 1f);
    if (Physics.Raycast(origin, (Vector3)direction, out RaycastHit hit, viewDistance))
    {
      Transform rootTransform = hit.transform.root;
      var isEnemy = rootTransform.CompareTag("Enemy");
      return isEnemy;
    }

    return false;
  }
  
  private void RemoveTarget(EnemyStateMachine target)
  {
    target.IsBeingFocused = false;
    if (target.FocusIndicatorImage != null)
    {
      target.FocusIndicatorImage.color = Colors.TransparentRed;
    }

    if (CurrentTarget == target) CurrentTarget = null;

    _targets.Remove(target);
  }
}
