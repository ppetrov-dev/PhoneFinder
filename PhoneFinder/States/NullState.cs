namespace PhoneFinder.States;

internal class NullState : StateBase
{
    public static NullState Null { get; } = new();

    private NullState()
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }
}
