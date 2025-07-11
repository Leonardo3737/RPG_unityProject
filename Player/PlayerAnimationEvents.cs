using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour, IAttackAnimationEvents
{
  public event Action<string, int> OnStartAttackEvent;
  public event Action OnEndAttackEvent;
  public event Action OnCancelAttackEvent;

  private bool EffectiveAttack;

  public bool GetEffectiveAttack() => EffectiveAttack;
  public void SetEffectiveAttack(bool effectiveAttack) => EffectiveAttack = effectiveAttack;

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

  public void OnCancelAttack()
  {
    OnCancelAttackEvent?.Invoke();
  }

  public void OnStartAttack(string AnimationName_ActionIndex)
  {
    var parts = AnimationName_ActionIndex.Split(',');

    string AnimationName = parts[0];
    int ActionIndex = parts.Count() > 1 ? int.Parse(parts[1]) : 999; // o valor 999 evita de executar a primeira ação

    if (!sm.VoiceAudioSource.isPlaying)
    {
      sm.MakeSound(sm.VoiceAudioSource, sm.AttackSounds[UnityEngine.Random.Range(0, sm.AttackSounds.Length)]);
    }

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

  public void OnTakingFootStep()
  {
    sm.MakeSound(sm.FootStepAudioSource, sm.FootStepSounds[UnityEngine.Random.Range(0, sm.FootStepSounds.Length)]);
  }
  public void OnJumpStart()
  {
    sm.MakeSound(sm.FootStepAudioSource, sm.JumpStartSounds[UnityEngine.Random.Range(0, sm.JumpStartSounds.Length)]);
  }

  public void OnJumpEnd()
  {
    sm.MakeSound(sm.FootStepAudioSource, sm.JumpEndSounds[UnityEngine.Random.Range(0, sm.JumpEndSounds.Length)]);
  }
  public void OnRoll()
  {
    sm.MakeSound(sm.FootStepAudioSource, sm.RollSounds[UnityEngine.Random.Range(0, sm.RollSounds.Length)]);
  }

}