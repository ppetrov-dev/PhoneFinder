using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PhoneFinder.Domain;
using PhoneFinder.Services;

namespace PhoneFinder.States;

internal class FoundGoalPhoneNumber : StateBase
{
    private readonly Account _account;
    private readonly PhoneNumber _phoneNumber;
    private readonly IStateMachine _stateMachine;
    private readonly ISoundService _soundService;
    private readonly ISmsService _smsService;
    private readonly IStateFactory _stateFactory;
    private readonly ILogger _logger;

    public FoundGoalPhoneNumber(
        Account account,
        PhoneNumber phoneNumber,
        IStateMachine stateMachine,
        ISoundService soundService,
        ILoggerFactory loggerFactory,
        ISmsService smsService,
        IStateFactory stateFactory)
    {
        _account = account;
        _phoneNumber = phoneNumber;
        _stateMachine = stateMachine;
        _soundService = soundService;
        _smsService = smsService;
        _stateFactory = stateFactory;

        _logger = loggerFactory.CreateLogger(typeof(FoundGoalPhoneNumber));
    }

    public override void Enter()
    {
        _logger.LogInformation("Enter for {0}", _phoneNumber);
        _logger.LogInformation("Found a goal number: {0}", _phoneNumber);

        _soundService.PlayGoalPhoneNumberFound();

        TryGetStatus(out _);
    }

    private bool TryGetStatus([NotNullWhen(true)] out string? code)
    {
        var getStatusResult = _smsService.GetStatusAsync(_account, _phoneNumber).GetAwaiter().GetResult();
        if (!getStatusResult.Success)
            Console.WriteLine(getStatusResult.Error);

        code = getStatusResult.Result;
        return getStatusResult.Success;
    }

    private bool IsOk { get; set; }

    public override void Exit()
    {
        _logger.LogInformation("Exit for {0}", _phoneNumber);

        if (!IsOk)
        {
            var rejectResult = _smsService.RejectAsync(_account, _phoneNumber).GetAwaiter().GetResult();
            if (rejectResult.Success)
                return;

            _soundService.PlayError();
            _logger.LogError("Error happened while rejecting the phone number: {0}", rejectResult.Error);
            Console.WriteLine("Возникла ошибка при отмене номера телефона: {0}", rejectResult.Error);

            return;
        }

        var successResult = _smsService.SuccessAsync(_account, _phoneNumber).GetAwaiter().GetResult();
        if (successResult.Success)
            return;

        _soundService.PlayError();
        _logger.LogError("Error happened while success the phone number: {0}", successResult.Error);
        Console.WriteLine("Возникла ошибка при подтверждении использования номера телефона: {0}", successResult.Error);
    }

    public override void Update()
    {
        base.Update();

        string? inputValue;
        string code;
        bool statusCodeReceived;

        do
        {
            Console.WriteLine(
                "Используйте номер ( {0}{1} ) для регистрации и нажмите Enter, чтобы проверить пришел ли код, или `next` для поиска следующего номера или `exit` для выхода:",
                _phoneNumber.Code,
                _phoneNumber.Number);

            inputValue = Console.ReadLine();
            Console.WriteLine("Запрос на получение статуса отправлен.");
            statusCodeReceived = TryGetStatus(out code);
        } while (string.IsNullOrWhiteSpace(inputValue)
                 && inputValue != "exit"
                 && inputValue != "next" && !statusCodeReceived);

        switch (inputValue)
        {
            case "exit":
                ExitRequested = true;
                return;
            case "next":
                _stateMachine.TransitionTo(_stateFactory.GetOrCreateAccountSelectedState(_account));
                return;
        }

        var message = $"Полученный код {code} по номеру {_phoneNumber.Code}{_phoneNumber.Number}";
        _logger.LogInformation(message);

        do
        {
            _soundService.PlayWarning();
            Console.WriteLine(message);
            Console.WriteLine("Введите `ok`, подтверждая использование кода для регистрации или 'exit' для выхода:");
            inputValue = Console.ReadLine();
        } while (inputValue is not "ok" or "exit");

        if (inputValue == "exit")
        {
            ExitRequested = true;
            return;
        }

        IsOk = true;
        _stateMachine.TransitionTo(_stateFactory.GetOrCreateAccountSelectedState(_account));
    }
}
