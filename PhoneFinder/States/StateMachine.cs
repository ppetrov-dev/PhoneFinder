namespace PhoneFinder.States;

internal class StateMachine : IStateMachine
{
    public IState CurrentState { get; private set; } = NullState.Null;

    public void TransitionTo(IState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
