using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour, IAttackAnimationEvents
{
  public event Action<string, int> OnStartAttackEvent;
  public event Action OnEndAttackEvent;
  public event Action OnCancelAttackEvent;

  private bool EffectiveAttack;

  public bool GetEffectiveAttack() => EffectiveAttack;
  public void SetEffectiveAttack(bool effectiveAttack) => EffectiveAttack = effectiveAttack;

  [field: SerializeField]
  private EnemyStateMachine sm;

  public void OnEnable()
  {
    sm.OnCancelAttackEvent += OnCancelAttack;
  }

  public void OnDisable()
  {
    sm.OnCancelAttackEvent -= OnCancelAttack;
  }

  public void OnCancelAttack()
  {
    OnCancelAttackEvent?.Invoke();
  }

  public void OnStartAttack(string AnimationName_ActionIndex)
  {
    var parts = AnimationName_ActionIndex.Split(',');

    string AnimationName = parts[0];
    int ActionIndex = parts.Count() > 1 ? int.Parse(parts[1]) : 999; // o valor 999 evita de executar a primeira ação


    sm.VoiceAudioSource.PlayOneShot(sm.AttackSounds[UnityEngine.Random.Range(0, sm.AttackSounds.Length)]);

    OnStartAttackEvent?.Invoke(AnimationName, ActionIndex);
  }

  public void OnEndAttack()
  {
    OnEndAttackEvent?.Invoke();
  }

  public void OnTakingFootStep()
  {
    sm.FootStepAudioSource.PlayOneShot(sm.FootStepSounds[UnityEngine.Random.Range(0, sm.FootStepSounds.Length)]);
  }

}