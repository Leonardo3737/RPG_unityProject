using System;

public class Attack<TypeSm> where TypeSm : StateMachine
{
  public int AnimationName { get; private set; }
  public float Start { get; private set; }
  public float CanMatch { get; private set; }
  public Action<TypeSm, float, float> Action;

  public Attack(
    int AnimationName,
    float Start,
    float CanMatch,
    Action<TypeSm, float, float> Action = null
  )
  {
    this.AnimationName = AnimationName;
    this.Start = Start;
    this.CanMatch = CanMatch;
    this.Action = Action;
  }
}