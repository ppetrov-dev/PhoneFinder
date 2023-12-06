using Microsoft.Extensions.Logging;
using PhoneFinder.Domain;
using PhoneFinder.Repositories;
using PhoneFinder.Services;

namespace PhoneFinder.States;

internal class StateFactory : IStateFactory
{
    private readonly IStateMachine _stateMachine;
    private readonly ISmsService _smsService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IGoalPhoneRangeRepository _goalPhoneRangeRepository;
    private readonly ISoundService _soundService;
    private readonly IAccountRepository _accountRepository;

    public StateFactory(
        IStateMachine stateMachine,
        ISmsService smsService,
        ILoggerFactory loggerFactory,
        IGoalPhoneRangeRepository goalPhoneRangeRepository,
        ISoundService soundService,
        IAccountRepository accountRepository)
    {
        _stateMachine = stateMachine;
        _smsService = smsService;
        _loggerFactory = loggerFactory;
        _goalPhoneRangeRepository = goalPhoneRangeRepository;
        _soundService = soundService;
        _accountRepository = accountRepository;
    }

    private readonly IDictionary<int, AccountSelectedState> _cache = new Dictionary<int, AccountSelectedState>();

    public IState GetOrCreateAccountSelectedState(Account account)
    {
        if (_cache.TryGetValue(account.Id, out var state))
            return state;

        var accountSelectedState = new AccountSelectedState(
            account,
            _stateMachine,
            _loggerFactory,
            _smsService,
            this,
            _goalPhoneRangeRepository,
            _soundService);

        _cache[account.Id] = accountSelectedState;
        return accountSelectedState;
    }

    public IState CreateFoundGoalPhoneNumber(Account account, PhoneNumber phoneNumber)
    {
        return new FoundGoalPhoneNumber(
            account,
            phoneNumber,
            _stateMachine,
            _soundService,
            _loggerFactory,
            _smsService,
            this);
    }

    public IState CreateNoBalanceState(Account account)
    {
        return new NoBalanceState(
            account,
            _stateMachine,
            _loggerFactory,
            this,
            _smsService,
            _soundService);
    }

    public IState CreateSelectNextAccountState(Account? currentAccount = null)
    {
        return new SelectNextAccountState(
            currentAccount,
            _stateMachine,
            _loggerFactory,
            _accountRepository,
            this,
            _soundService);
    }
}
