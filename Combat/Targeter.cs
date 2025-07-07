using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Targeter<TargetType> : MonoBehaviour where TargetType : StateMachine
{
  [field: SerializeField]
  public List<TargetType> _targets = new();

  [field: SerializeField]
  public TargetType CurrentTarget { get; protected set; }

  public event Action OnTriggerEnterEvent;
  public abstract bool HasLineOfSight();

  private int VerificationIndex = 0;

  public virtual void OnTriggerEnter(Collider other)
  {
    if (!other.TryGetComponent(out TargetType target)) return;

    _targets.Add(target);
    OnTriggerEnterEvent?.Invoke();
  }

  public virtual void OnTriggerExit(Collider other)
  {
    if (!other.TryGetComponent(out TargetType target)) return;

    if (CurrentTarget == target) CurrentTarget = null;

    _targets.Remove(target);
  }

  public bool SelectClosestTarget(Vector3 position)
  {
    if (_targets.Count == 0) return false;
    TargetType closest = null;

    float minDistance = Mathf.Infinity;

    foreach (var target in _targets)
    {
      if (target == null) continue;

      float distance = Vector3.Distance(position, target.transform.position);

      if (distance < minDistance)
      {
        minDistance = distance;
        closest = target;
      }
    }

    CurrentTarget = closest;
    return true;
  }

  public bool SelectTarget()
  {
    if (_targets.Count == 0)
    {
      return false;
    }

    if (CurrentTarget != null && HasLineOfSight())
    {
      return true;
    }

    CurrentTarget = _targets[0];

    if (!HasLineOfSight())
    {
      if (_targets.Count == 1)
      {
        return false;
      }
      else
      {
        return ChangeTarget();
      }
    }

    return true;
  }

  public virtual bool ChangeTarget()
  {
    if (_targets.Count == 0 || VerificationIndex + 1 == _targets.Count)
    {
      VerificationIndex = 0;
      return false;
    }

    if (CurrentTarget == null)
    {
      CurrentTarget = _targets[0];
      return HasLineOfSight();
    }

    var index = _targets.IndexOf(CurrentTarget);

    if (index + 1 >= _targets.Count)
    {
      CurrentTarget = _targets[0];
    }
    else
    {
      CurrentTarget = _targets[index + 1];
    }

    if (!HasLineOfSight() && VerificationIndex + 1 < _targets.Count)
    {
      VerificationIndex++;
      return ChangeTarget();
    }

    return true;

  }


  public Vector3? GetTargetPosition()
  {
    if (CurrentTarget == null)
    {
      return null;
    }
    Vector3 targetPosition = CurrentTarget.transform.position;

    if (CurrentTarget.TryGetComponent(out CharacterController targetController))
    {
      targetPosition += Vector3.up * targetController.height;
    }
    return targetPosition;
  }

}