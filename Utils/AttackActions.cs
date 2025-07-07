using System;
using UnityEngine;

public static class AttackActions<TypeSm> where TypeSm : StateMachine
{
  public static void MainAttack3Action(TypeSm sm, float deltaTime, float normalizedTime)
  {
    if (normalizedTime >= 0.13f && normalizedTime < 0.24f)
    {
      Vector3 move = sm.transform.forward * (8f * deltaTime);
      sm.Controller.Move(move);
      return;
    }
    if (normalizedTime >= 0.83f && normalizedTime < 0.91f)
    {
      Vector3 move = -sm.transform.forward * (2.5f * deltaTime);
      sm.Controller.Move(move);
      return;
    }
  }

  public static void AlternativeAttack3Action(TypeSm sm, float deltaTime, float normalizedTime)
  {
    if (normalizedTime >= 0.43f && normalizedTime < 0.53f)
    {
      sm.ForceReceiver.VerticalVelocity = 0.1f;
    }

    if (normalizedTime >= 0.2f && normalizedTime < 0.65f)
    {
      Vector3 move = sm.transform.forward * (4f * deltaTime);
      sm.Controller.Move(move);
      return;
    }
  }

  public static void AlternativeAttack1Action(TypeSm sm, float deltaTime, float normalizedTime)
  {
    if (normalizedTime >= 0.43f && normalizedTime < 0.53f)
    {
      Vector3 move = sm.transform.forward * (2f * deltaTime);
      sm.Controller.Move(move);
      return;
    }
    if (normalizedTime >= 0.53f && normalizedTime < 0.8f)
    {
      Vector3 move = sm.transform.forward * (0.5f * deltaTime);
      sm.Controller.Move(move);
      return;
    }
  }
}