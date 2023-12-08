namespace PhoneFinder.States;

internal abstract class StateBase : IState
{
    public abstract void Enter();

    public abstract void Exit();

    public virtual void Update()
    {
    }

    public bool ExitRequested { get; protected set; }
}
