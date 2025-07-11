using System;
using UnityEngine;

public abstract class EnemyBaseState : State
{
	protected EnemyStateMachine sm;

	protected static readonly int FreeLookBlendTree = Animator.StringToHash("FreeLookBlendTree");
	protected static readonly int FreeLookSpeed = Animator.StringToHash("FreeLookSpeed");

	protected float CurrentAnimationSmooth;
	protected float CurrentAnimationVelocity;

	public EnemyBaseState(EnemyStateMachine stateMachine, StatesType statesType)
	{
		StateType = statesType;
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

	public bool HasReachedDestination()
	{
		return !sm.NavMeshAgent.pathPending &&
					 sm.NavMeshAgent.remainingDistance <= sm.NavMeshAgent.stoppingDistance &&
					 (!sm.NavMeshAgent.hasPath || sm.NavMeshAgent.velocity.sqrMagnitude < 0.01f);
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

	public void CheckIsPlayerVisible()
	{
		if (!sm.Targeter.SelectTarget() || !sm.Targeter.HasLineOfSight() || !sm.IsPatrol) return;

		sm.ChangeState(new EnemyChaseState(sm));
	}

	public bool CanAttack()
	{
		var target = sm.Targeter.CurrentTarget;
		
		if (target == null) return false;

		var distance = Vector3.Distance(target.transform.position, sm.transform.position);

		return distance < sm.MaxAttackDistance;
	}


}
