using Microsoft.Extensions.Logging;
using PhoneFinder.Services;
using PhoneFinder.States;

namespace PhoneFinder;

internal class ApplicationRunner : IApplicationRunner
{
    private readonly ILogger _logger;
    private readonly IStateMachine _stateMachine;
    private readonly IStateFactory _stateFactory;
    private readonly IBalanceChecker _balanceChecker;

    public ApplicationRunner(
        ILoggerFactory loggerFactory,
        IStateMachine stateMachine,
        IStateFactory stateFactory,
        IBalanceChecker balanceChecker)
    {
        _logger = loggerFactory.CreateLogger(typeof(ApplicationRunner));
        _stateMachine = stateMachine;
        _stateFactory = stateFactory;
        _balanceChecker = balanceChecker;
    }

    public void Run()
    {
        _logger.LogInformation("Begin");
        try
        {
            _balanceChecker.RequestAndShowBalancesAsync().GetAwaiter().GetResult();

            _stateMachine.TransitionTo(_stateFactory.CreateSelectNextAccountState());

            while (!_stateMachine.CurrentState.ExitRequested)
                _stateMachine.CurrentState.Update();

            _balanceChecker.RequestAndShowBalancesAsync().GetAwaiter().GetResult();
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
