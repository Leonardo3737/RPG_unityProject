using System;
using System.Linq;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
  public event Action<string, int> OnStartAttackEvent;
  public event Action OnEndAttackEvent;
  public event Action OnCancelAttackEvent;

  public bool EffectiveAttack;

  [field: SerializeField]
  private PlayerStateMachine sm;

  public void OnEnable()
  {
    sm.OnCancelAttackEvent += OnCancelAttack;
  }
  
  public void OnDisable()
  {
    sm.OnCancelAttackEvent -= OnCancelAttack;
  }

  public void OnCancelAttack() {
    OnCancelAttackEvent.Invoke();
  }

  public void OnStartAttack(string AnimationName_ActionIndex)
  {
    var parts = AnimationName_ActionIndex.Split(',');

    string AnimationName = parts[0];
    int ActionIndex = parts.Count() > 1 ? int.Parse(parts[1]) : 999; // o valor 999 evita de executar a primeira ação

    OnStartAttackEvent?.Invoke(AnimationName, ActionIndex);
  }

  public void OnEndAttack()
  {
    OnEndAttackEvent?.Invoke();
  }

  public void OnStartCombo()
  {
    if (!EffectiveAttack)
    {
      sm.ChangeState(new PlayerFreeLookState(sm));
    }
  }
}