using System;
using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine sm;

    protected static readonly int FreeLookBlendTree = Animator.StringToHash("FreeLookBlendTree");
    protected static readonly int FreeLookSpeed = Animator.StringToHash("FreeLookSpeed");

    protected float CurrentAnimationSmooth;
    protected float CurrentAnimationVelocity;

    public EnemyBaseState(EnemyStateMachine stateMachine)
    {
        sm = stateMachine;
    }

    public virtual bool FaceMoveDirection(float deltaTime)
    {
        Vector3 moveDir = sm.NavMeshAgent.velocity.normalized;

        if (moveDir == Vector3.zero)
            moveDir = sm.transform.forward;

        Quaternion lookRotation = Quaternion.LookRotation(moveDir);
        sm.transform.rotation = Quaternion.Slerp(
            sm.transform.rotation,
            lookRotation,
            sm.RotationSpeed * deltaTime
        );

        return Quaternion.Angle(sm.transform.rotation, lookRotation) < 0.1f;
    }

    protected bool HasReachedDestination()
    {
        if (!sm.NavMeshAgent.pathPending) // o caminho já foi calculado
        {
            if (sm.NavMeshAgent.remainingDistance <= sm.NavMeshAgent.stoppingDistance)
            {
                if (!sm.NavMeshAgent.hasPath || sm.NavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void RunAnimation(float deltaTime, float animationSpeed)
    {

        CurrentAnimationSmooth = Mathf.SmoothDamp(
            CurrentAnimationSmooth,
            animationSpeed,
            ref CurrentAnimationVelocity,
            0.2f
        );

        sm.Animator.SetFloat(FreeLookSpeed, CurrentAnimationSmooth, 0.1f, deltaTime);
    }

    protected float GetNormalizedTime(Animator animator, string tag)
    {
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        var nextState = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) && nextState.IsTag(tag))
        {
            return nextState.normalizedTime;
        }
        if (currentState.IsTag(tag))
        {
            return currentState.normalizedTime;
        }
        return 0f;
    }
}
