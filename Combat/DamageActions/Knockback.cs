using UnityEngine;

public class Knockback : DamageAction
{
  public override void Run(float deltaTime, float normalizedTime, StateMachine sm)
  {
    if (normalizedTime < 0.65f)
    {
      float knockbackStrength = Mathf.Lerp(3f, 0f, normalizedTime); // força decresce de 3 para 0
      Vector3 move = -sm.transform.forward * (knockbackStrength * deltaTime);
      sm.Controller.Move(move);
    }
  }
}