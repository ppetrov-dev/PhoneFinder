namespace PhoneFinder.States;

internal interface IStateMachine
{
    IState CurrentState { get; }
    void TransitionTo(IState newState);
}
