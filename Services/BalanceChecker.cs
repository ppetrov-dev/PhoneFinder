using PhoneFinder.Domain;
using PhoneFinder.Repositories;

namespace PhoneFinder.Services;

internal class BalanceChecker : IBalanceChecker
{
    private readonly IAccountRepository _accountRepository;
    private readonly ISmsService _smsService;

    public BalanceChecker(
        IAccountRepository accountRepository,
        ISmsService smsService)
    {
        _accountRepository = accountRepository;
        _smsService = smsService;
    }

    public async Task RequestAndShowBalancesAsync()
    {
        await foreach (var tuple in GetBalancesAsync())
            Console.WriteLine("#{0}-{1}:{2}", tuple.Item1.Id, tuple.Item1.Name, tuple.Item2);
    }

    private async IAsyncEnumerable<Tuple<Account, decimal>> GetBalancesAsync()
    {
        foreach (var account in _accountRepository.Items)
        {
            var serverResult = await _smsService.GetBalanceAsync(account);
            yield return new Tuple<Account, decimal>(account, serverResult.Result);
        }
    }
}
