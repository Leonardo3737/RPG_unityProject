using System;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    [field: SerializeField]
    public CharacterController Controller { get; private set; }

    [field: SerializeField]
    public ForceReceiver ForceReceiver { get; private set; }

    protected State currentState;

    public virtual void ChangeState(State newState)
    {
        newState.PreviousStateType = currentState?.StateType;

        currentState?.Exit();
        currentState = newState;

        currentState.Enter();
    }

    public virtual void Update()
    {
        currentState?.Update(Time.deltaTime);
    }

    public abstract void OnDamage(int WeaponDamage, string AnimationName, DamageAction Action);
}
