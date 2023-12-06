using PhoneFinder.Domain;

namespace PhoneFinder.Repositories;

internal interface IGoalPhoneRangeRepository : IRepository<PhoneRange>
{
    bool GetIsOk(PhoneNumber phoneNumber);
}
