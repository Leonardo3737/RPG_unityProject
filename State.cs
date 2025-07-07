public abstract class State
{
    public StatesType StateType;
    public StatesType? PreviousStateType;

    public abstract void Enter();
    public abstract void Update(float deltaTime);
    public abstract void Exit();

    public abstract bool CanPerformAction();
}
