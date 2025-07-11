using System;

public interface IAttackAnimationEvents
{
  public bool GetEffectiveAttack();
  public void SetEffectiveAttack(bool effectiveAttack);
  event Action<string, int> OnStartAttackEvent;
  event Action OnEndAttackEvent;
  event Action OnCancelAttackEvent;
}