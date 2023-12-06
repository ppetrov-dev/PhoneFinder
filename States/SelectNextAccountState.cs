using Microsoft.Extensions.Logging;
using PhoneFinder.Domain;
using PhoneFinder.Repositories;
using PhoneFinder.Services;

namespace PhoneFinder.States;

internal class SelectNextAccountState : StateBase
{
    private readonly Account? _currentAccount;
    private readonly IStateMachine _stateMachine;
    private readonly IAccountRepository _accountRepository;
    private readonly IStateFactory _stateFactory;
    private readonly ISoundService _soundService;
    private readonly ILogger _logger;

    public SelectNextAccountState(
        Account? currentAccount,
        IStateMachine stateMachine,
        ILoggerFactory loggerFactory,
        IAccountRepository accountRepository,
        IStateFactory stateFactory,
        ISoundService soundService)
    {
        _currentAccount = currentAccount;
        _stateMachine = stateMachine;
        _accountRepository = accountRepository;
        _stateFactory = stateFactory;
        _soundService = soundService;
        _logger = loggerFactory.CreateLogger(typeof(SelectNextAccountState));
    }

    public override void Enter()
    {
        if (_currentAccount is not null)
            _logger.LogInformation("Enter for (#{0} - {1})", _currentAccount.Id, _currentAccount.Name);

        _logger.LogInformation("Enter for {0} when current account is null", nameof(SelectNextAccountState));
    }

    public override void Exit()
    {
        if (_currentAccount is not null)
            _logger.LogInformation("Exit for (#{0} - {1})", _currentAccount.Id, _currentAccount.Name);

        _logger.LogInformation("Exit for {0} when current account is null", nameof(SelectNextAccountState));
    }

    public override void Update()
    {
        base.Update();

        var selectedAccount = _currentAccount is null
            ? _accountRepository.Items.FirstOrDefault()
            : _accountRepository.Items.SkipWhile(item => item != _currentAccount).Skip(1).FirstOrDefault();

        if (selectedAccount is null)
        {
            Console.WriteLine("Невозможно выбрать аккаунт. Выберите вручную из списка по Id:");
            var accounts = string.Join('\n', _accountRepository.Items);
            Console.WriteLine(accounts);

            string? inputId;
            int id;
            do
            {
                _soundService.PlayWarning();
                Console.WriteLine("Введите Id (число) или `exit` для выхода: ");
                inputId = Console.ReadLine();

                if (inputId != "exit")
                    continue;

                ExitRequested = true;
                return;
            } while (!int.TryParse(inputId, out id));

            selectedAccount = _accountRepository.Resolve(id);
        }

        Console.WriteLine("Выбран аккаунт: #{0} - {1}", selectedAccount.Id, selectedAccount.Name);
        _logger.LogInformation("Selected account: #{0} - {1}", selectedAccount.Id, selectedAccount.Name);
        _stateMachine.TransitionTo(_stateFactory.GetOrCreateAccountSelectedState(selectedAccount));
    }
}
