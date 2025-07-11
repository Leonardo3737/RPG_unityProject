using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
  private static List<Attack<PlayerStateMachine>> CurrentAttacks;
  private static readonly List<Attack<PlayerStateMachine>> MainAttacks = new()
        {
          new(Animator.StringToHash("Attack-1"), 0.01f, 0.55f),
          new(Animator.StringToHash("Attack-2"), 0.2f, 0.46f),
          new(Animator.StringToHash("Attack-3"), 0.01f, 1f, AttackActions<PlayerStateMachine>.MainAttack3Action),
        };

  private static readonly List<Attack<PlayerStateMachine>> AlternativeAttacks = new()
  {
    new(Animator.StringToHash("AlternativeAttack-2"), 0.2f, 0.6f),
    new(Animator.StringToHash("AlternativeAttack-1"), 0.3f, 0.6f, AttackActions<PlayerStateMachine>.AlternativeAttack1Action),
    new(Animator.StringToHash("AlternativeAttack-3"), 0.15f, 1f, AttackActions<PlayerStateMachine>.AlternativeAttack3Action),
  };

  private static readonly int RunToAttackAnimation = Animator.StringToHash("RunToAttack");

  private Attack<PlayerStateMachine> Attack;

  private bool WalkAnimationIsStarted;

  public PlayerAttackState(
    PlayerStateMachine stateMachine,
    AttackTypes AttackType
    ) : base(stateMachine, StatesType.ATTACK)
  {
    switch (AttackType)
    {

      case AttackTypes.MainAttack:
        CurrentAttacks = MainAttacks;
        break;

      case AttackTypes.AlternativeAttack:
        CurrentAttacks = AlternativeAttacks;
        break;

    }
  }

  public override void Enter()
  {

    if (!sm.IsTriggered)
    {
      EnableCameraInputController();
    }

    if (PreviousStateType == StatesType.ATTACK)
    {
      sm.AttackIndex++;
    }
    else
    {
      sm.AttackIndex = 0;
    }

    if (CurrentAttacks.Count < sm.AttackIndex + 1)
    {
      EndCombo();
      return;
    }

    Attack = CurrentAttacks[sm.AttackIndex];

    sm.Animator.CrossFadeInFixedTime(Attack.AnimationName, 0.1f, sm.UnequippedLayer, Attack.Start);
  }

  public override void Update(float deltaTime)
  {

    var normalizedTime = GetNormalizedTime(sm.Animator, "attack");
    if (sm.Targeter.SelectTarget() && normalizedTime < Attack.Start + 0.05f)
    {
      if (!sm.IsTriggered)
      {
        sm.Targeter.SelectClosestTarget(sm.transform.position);
      }

      var targetPos = sm.Targeter.GetTargetPosition().Value;
      var selfPos = sm.transform.position;

      float distance = Vector3.Distance(selfPos, targetPos);

      var limitDistance = sm.IsTriggered ? sm.MaxAttackDistanceOnTrigger : sm.MaxAttackDistance;

      if (distance < limitDistance)
      {
        PlayerLookToTarget();
        TriggerPositionCamera(deltaTime);
        PositionCameraTargetInTarget(deltaTime, false);
        var isWalk = PlayerMoveToTarget(distance, selfPos, targetPos, deltaTime);
        if (!isWalk && normalizedTime == 0f)
        {
          sm.Animator.CrossFadeInFixedTime(Attack.AnimationName, 0.1f, sm.UnequippedLayer, Attack.Start);
        }
      }
      else if (WalkAnimationIsStarted)
      {
        EndCombo();
      }
    }

    FreeLookMove(deltaTime, Vector3.zero);


    // REALIZA DO ATAQUE AÇÃO CASO TENHA
    Attack.Action?.Invoke(sm, deltaTime, normalizedTime);

    if (normalizedTime >= 1f)
    {
      EndCombo();
    }
  }

  public override void Exit()
  {
    if (sm.CancelAttack)
    {
      sm.Invoke("ResetCancelAttack", 0.2f);
    }
    
    var childTransform = sm.transform.Find("Erika");
    childTransform.rotation = sm.transform.rotation;
    if (sm.IsTriggered)
    {
      PlayerLookToTarget();
    }
  }

  public override bool CanPerformAction()
  {
    var animationTime = GetNormalizedTime(sm.Animator, "attack");

    return animationTime >= Attack.CanMatch || sm.CancelAttack;
  }

  public void EndCombo()
  {
    if (sm.IsTriggered)
    {
      sm.ChangeState(new PlayerTriggerState(sm));
    }
    else
    {
      sm.ChangeState(new PlayerFreeLookState(sm));
    }
  }

  private bool PlayerMoveToTarget(float distance, Vector3 selfPos, Vector3 targetPos, float deltaTime)
  {
    var IsWalk = false;
    if (sm.Targeter.HasLineOfSight())
    {
      IsWalk = distance > 2.1f && sm.AttackIndex != 2;
      
      if (IsWalk)
      {
        if (!WalkAnimationIsStarted)
        {
          sm.Animator.CrossFadeInFixedTime(RunToAttackAnimation, 0.01f, sm.UnequippedLayer);
          WalkAnimationIsStarted = true;
        }
        sm.Animator.SetFloat(FreeLookSpeed, 1f);

        float approachSpeed = 9f;
        Vector3 moveDirection = (targetPos - selfPos).normalized;
        moveDirection.y = 0;
        Vector3 movement = moveDirection * (approachSpeed * deltaTime);

        sm.Controller.Move(movement);
        IsWalk = true;
      }
    }
    return IsWalk;
  }

  private void PlayerLookToTarget()
  {
    if (sm.Targeter.HasLineOfSight())
    {
      var transform = sm.transform;
      var targetPosition = sm.Targeter.GetTargetPosition().Value;

      var direction = targetPosition - sm.transform.position;
      direction.y = 0;
      transform.rotation = Quaternion.LookRotation(direction);
    }
  }
}