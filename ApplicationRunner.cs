using Microsoft.Extensions.Logging;
using PhoneFinder.States;

namespace PhoneFinder;

internal class ApplicationRunner : IApplicationRunner
{
    private readonly ILogger _logger;
    private readonly IStateMachine _stateMachine;
    private readonly IStateFactory _stateFactory;

    public ApplicationRunner(
        ILoggerFactory loggerFactory,
        IStateMachine stateMachine,
        IStateFactory stateFactory)
    {
        _logger = loggerFactory.CreateLogger(typeof(ApplicationRunner));
        _stateMachine = stateMachine;
        _stateFactory = stateFactory;
    }

    public void Run()
    {
        _logger.LogInformation("Begin");
        try
        {
            _stateMachine.TransitionTo(_stateFactory.CreateSelectNextAccountState());

            while (!_stateMachine.CurrentState.ExitRequested)
                _stateMachine.CurrentState.Update();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception happened");
            Console.WriteLine($"Unhandled exception happened: {exception.Message}");
        }
        finally
        {
            _logger.LogInformation("End");
        }
    }
}
