using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerToggleModesState : PlayerBaseState
{
  private static int IsToggleMode;
  private bool IsToggle;
  private bool IsDone;
  private readonly int ToggleModelLayer = 3;

  public PlayerToggleModesState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.TOGGLE_MODE)
  {
    IsToggleMode = sm.CurrentMode == Modes.EQUIPPED ? Animator.StringToHash("isUnequipping") : Animator.StringToHash("isEquipping");
  }

  public override void Enter()
  {
    StateType = StatesType.TOGGLE_MODE;

    var currentAnimatorStateInfo = sm.Animator.GetCurrentAnimatorStateInfo(sm.CurrentLayer);
    if (!sm.IsTriggered && currentAnimatorStateInfo.shortNameHash != FreeLookBlendTree)
    {
      sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f, sm.CurrentLayer);
    }
    if (sm.IsTriggered && currentAnimatorStateInfo.shortNameHash != TriggerBlendTree)
    {
      sm.Animator.CrossFadeInFixedTime(TriggerBlendTree, 0.1f, sm.CurrentLayer);
    }

    sm.Animator.SetLayerWeight(ToggleModelLayer, 1f);
    sm.Animator.SetBool(IsToggleMode, true);
  }

  public override void Update(float deltaTime)
  {
    if (sm.IsTriggered)
    {
      TriggerMovement(deltaTime);
    }
    else
    {
      FreeLookMovement(deltaTime);
    }

    var normalizedTime = GetNormalizedTime(sm.Animator, "toggleModes", ToggleModelLayer);

    if (normalizedTime > 0.37f && !IsToggle)
    {
      sm.ToggleMode();
      IsToggle = true;
    }

    if (normalizedTime > 0.6f && !IsDone)
    {
      sm.ToggleAnimatorController();
      sm.Animator.SetBool(IsToggleMode, false);
      IsDone = true;
      End();
    }
  }

  public override void Exit()
  {
    if (IsToggle && !IsDone)
    {
      sm.ToggleAnimatorController();
      sm.Animator.SetBool(IsToggleMode, false);
    }
    sm.Animator.SetLayerWeight(ToggleModelLayer, 0);
  }

  public override bool CanPerformAction()
  {
    return false;
  }

  private void End()
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
}