using UnityEngine;

public abstract class DamageAction : MonoBehaviour
{
  public abstract void Run(float deltaTime, float normalizedTime, StateMachine sm);
}