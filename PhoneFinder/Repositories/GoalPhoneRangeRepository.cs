using Newtonsoft.Json;
using PhoneFinder.Domain;
using PhoneFinder.Services;

namespace PhoneFinder.Repositories;

internal class GoalPhoneRangeRepository : IGoalPhoneRangeRepository
{
    private readonly Dictionary<int, PhoneRange> _dictionary;
    private const string GoalsJsonFilePath = "goals.json";

    public GoalPhoneRangeRepository(IPathService pathService)
    {
        var fullPathToResource = pathService.GetFullPathToResource(GoalsJsonFilePath);
        if (!File.Exists(fullPathToResource))
            throw new FileNotFoundException(
                $"Place {GoalsJsonFilePath} with the searching region phone ranges from https://opendata.digital.gov.ru/registry/numeric next to the exe file");

        var jsonText = File.ReadAllText(fullPathToResource);
        var jsonDataWrapper = JsonConvert.DeserializeObject<JsonDataWrapper<PhoneRange>>(jsonText);
        Items = jsonDataWrapper?.Data ?? Enumerable.Empty<PhoneRange>();

        _dictionary = Items.ToDictionary(range => range.Id);
    }

    public bool GetIsOk(PhoneNumber phoneNumber)
    {
        return Items.Any(
            range =>
            {
                var number = int.Parse(phoneNumber.Number);
                return range.Code == int.Parse(phoneNumber.Code)
                       && range.Begin <= number && range.End >= number;
            });
    }

    public IEnumerable<PhoneRange> Items { get; }

    public bool Contains(int id) => _dictionary.ContainsKey(id);

    public PhoneRange Resolve(int id) => _dictionary[id];
}
