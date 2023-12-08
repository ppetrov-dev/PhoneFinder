namespace PhoneFinder.States;

internal interface IState
{
    void Enter();

    void Exit();

    void Update();

    bool ExitRequested { get; }
}
