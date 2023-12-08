using Microsoft.Extensions.Logging;
using PhoneFinder.Domain;
using PhoneFinder.Repositories;
using PhoneFinder.Services;

namespace PhoneFinder.States;

internal class AccountSelectedState : StateBase
{
    private readonly Account _account;
    private readonly IStateMachine _stateMachine;
    private readonly ILogger _logger;
    private readonly ISmsService _smsService;
    private readonly IStateFactory _stateFactory;
    private readonly IGoalPhoneRangeRepository _goalPhoneRangeRepository;
    private readonly ISoundService _soundService;

    public AccountSelectedState(
        Account account,
        IStateMachine stateMachine,
        ILoggerFactory loggerFactory,
        ISmsService smsService,
        IStateFactory stateFactory,
        IGoalPhoneRangeRepository goalPhoneRangeRepository,
        ISoundService soundService)
    {
        _account = account;
        _stateMachine = stateMachine;

        _logger = loggerFactory.CreateLogger(typeof(AccountSelectedState));
        _smsService = smsService;
        _stateFactory = stateFactory;
        _goalPhoneRangeRepository = goalPhoneRangeRepository;
        _soundService = soundService;
    }

    private int Index { get; set; } = 1;

    public override void Enter()
    {
        _logger.LogInformation("Enter for (#{0} - {1})", _account.Id, _account.Name);
    }

    public override void Exit()
    {
        _logger.LogInformation("Exit for (#{0} - {1})", _account.Id, _account.Name);
    }

    public override void Update()
    {
        base.Update();
        Console.WriteLine("Запрос {0}: #{1} - {2}", Index, _account.Id, _account.Name);

        var serverResult = _smsService.GetPhoneNumberAsync(_account)
            .GetAwaiter()
            .GetResult();

        if (!serverResult.Success)
        {
            switch (serverResult.Error.ErrorCode)
            {
                case ErrorCode.NO_BALANCE:
                    _stateMachine.TransitionTo(_stateFactory.CreateNoBalanceState(_account));
                    return;
                case ErrorCode.ERROR_SQL:
                    _stateMachine.TransitionTo(_stateFactory.CreateSelectNextAccountState(_account));
                    break;
            }

            _soundService.PlayError();
            _logger.LogError("Error happened while getting a new phone number: {0}", serverResult.Error);
            Console.WriteLine("Возникла ошибка при получении номера: {0}", serverResult.Error);
            return;
        }

        var phoneNumber = serverResult.Result;

        Console.WriteLine("Проверка номера: {0}-{1}", phoneNumber.Code, phoneNumber.Number);

        if (_goalPhoneRangeRepository.GetIsOk(phoneNumber))
            _stateMachine.TransitionTo(_stateFactory.CreateFoundGoalPhoneNumber(_account, phoneNumber));
        else
        {
            Console.Write("Результат: номер не подходит. ");
            var rejectNumberResult = _smsService.RejectAsync(_account, phoneNumber).GetAwaiter().GetResult();

            var message = "Номер успешно деактивирован.";
            if (!rejectNumberResult.Success)
            {
                _soundService.PlayError();
                message = $"Возникла ошибка деактивации {rejectNumberResult.Error}.";
                _logger.LogError(message);
            }

            Console.WriteLine(message);
        }

        Index++;
    }
}
