using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhoneFinder.Domain;
using PhoneFinder.Services;

namespace PhoneFinder.Repositories;

internal class AccountRepository : IAccountRepository
{
    private readonly Dictionary<int, Account> _dictionary;
    private const string AccountsJsonFilePath = "accounts.json";

    public AccountRepository(
        IPathService pathService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(AccountRepository));
        var fullPathToResource = pathService.GetFullPathToResource(AccountsJsonFilePath);
        if (!File.Exists(fullPathToResource))
            throw new FileNotFoundException($"Place {AccountsJsonFilePath} with accounts of sms services next to the exe file");

        var jsonText = File.ReadAllText(fullPathToResource);
        var jsonDataWrapper = JsonConvert.DeserializeObject<JsonDataWrapper<Account>>(jsonText);
        Items = jsonDataWrapper?.Data ?? throw new ArgumentException("Accounts cannot be empty");
        _dictionary = Items.ToDictionary(account => account.Id);

        Console.WriteLine("Loaded accounts {0}", _dictionary.Count);
        logger.LogInformation("Loaded accounts {0}", _dictionary.Count);
    }

    public IEnumerable<Account> Items { get; }

    public bool Contains(int id) => _dictionary.ContainsKey(id);

    public Account Resolve(int id) => _dictionary[id];
}
