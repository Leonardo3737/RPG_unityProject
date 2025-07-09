using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, Controls.IPlayerActions
{
    public Controls controls;
    public Vector2 InputMovement { get; private set; }
    public event Action OnMainAttackEvent;
    public event Action OnAlternativeAttackEvent;
    public event Action OnTriggerEvent;
    public event Action OnChangeTriggerEvent;
    public event Action OnRollEvent;
    public event Action OnToggleModesEvent;
    public event Action OnJumpEvent;

    void OnEnable()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        InputMovement = context.ReadValue<Vector2>();
    }

    public void OnMainAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnMainAttackEvent?.Invoke();
    }

    public void OnAlternativeAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnAlternativeAttackEvent?.Invoke();
    }

    public void OnTrigger(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnTriggerEvent?.Invoke();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnRollEvent?.Invoke();
    }

    public void OnChangeTrigger(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnChangeTriggerEvent?.Invoke();
    }

    public void OnToggleModes(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnToggleModesEvent?.Invoke();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnJumpEvent?.Invoke();
    }



}
