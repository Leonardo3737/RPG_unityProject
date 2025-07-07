using UnityEngine;

public class EnemyTargeter : Targeter<PlayerStateMachine>
{
  [field: SerializeField]
  public float ViewAngle = 120f;

  public override bool HasLineOfSight()
  {
    var targetPosition = GetTargetPosition();

    var origin = transform.position;

    var direction = (targetPosition - origin)?.normalized;

    if (direction == null)
    {
      return false;
    }

    float angleToTarget = Vector3.Angle(transform.forward, (Vector3)direction);

    if (angleToTarget > ViewAngle / 2f)
    {
      // Está fora do campo de visão
      return false;
    }

    origin += (Vector3)direction;

    var viewDistance = 0f;
    if (TryGetComponent(out CapsuleCollider capsuleCollider))
    {

      viewDistance = capsuleCollider.radius;
    }
    //Debug.DrawRay(origin, direction * viewDistance, Color.red, 1f);
    if (Physics.Raycast(origin, (Vector3)direction, out RaycastHit hit, viewDistance))
    {
      Transform rootTransform = hit.transform.root;
      return rootTransform.CompareTag("Player");
    }
    return false;
  }
}