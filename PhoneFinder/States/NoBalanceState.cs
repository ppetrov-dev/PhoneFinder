using Microsoft.Extensions.Logging;
using PhoneFinder.Domain;
using PhoneFinder.Services;

namespace PhoneFinder.States;

internal class NoBalanceState : StateBase
{
    private readonly Account _account;
    private readonly IStateMachine _stateMachine;
    private readonly IStateFactory _stateFactory;
    private readonly ISmsService _smsService;
    private readonly ISoundService _soundService;
    private readonly ILogger _logger;

    public NoBalanceState(
        Account account,
        IStateMachine stateMachine,
        ILoggerFactory loggerFactory,
        IStateFactory stateFactory,
        ISmsService smsService,
        ISoundService soundService)
    {
        _account = account;
        _stateMachine = stateMachine;
        _stateFactory = stateFactory;
        _smsService = smsService;
        _soundService = soundService;
        _logger = loggerFactory.CreateLogger(typeof(NoBalanceState));
    }

    public override void Enter()
    {
        _logger.LogInformation("Enter for (#{0} - {1})", _account.Id, _account.Name);

        RequestBalance();
    }

    public override void Exit()
    {
        _logger.LogInformation("Exit for (#{0} - {1})", _account.Id, _account.Name);
    }

    public override void Update()
    {
        base.Update();

        string? inputValue;

        do
        {
            _soundService.PlayWarning();
            Console.WriteLine(
                "Пополните баланс у аккаунта (#{0} - {1}) и введите `ok` или `next` для перехода к следующему или `exit` для выхода:",
                _account.Id,
                _account.Name);

            inputValue = Console.ReadLine();
        } while (inputValue is not "next" and not "ok" and not "exit");

        switch (inputValue)
        {
            case "exit":
                ExitRequested = true;
                return;
            case "next":
                _stateMachine.TransitionTo(_stateFactory.CreateSelectNextAccountState(_account));
                return;
        }

        _stateMachine.TransitionTo(_stateFactory.GetOrCreateAccountSelectedState(_account));
    }

    private void RequestBalance()
    {
        var getBalanceResult = _smsService.GetBalanceAsync(_account).GetAwaiter().GetResult();
        if (getBalanceResult.Success)
        {
            Console.WriteLine("Текущий баланс: {0}", getBalanceResult.Result);
            return;
        }

        _soundService.PlayError();
        _logger.LogError("Error happened while getting the balance: {0}", getBalanceResult.Error);
        Console.WriteLine("Возникла ошибка при запросе баланса: {0}", getBalanceResult.Error);
    }
}
