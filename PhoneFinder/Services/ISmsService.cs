using PhoneFinder.Domain;

namespace PhoneFinder.Services;

internal interface ISmsService
{
    string BaseUrl { get; }

    Task<ServerResult<PhoneNumber>> GetPhoneNumberAsync(Account account);

    Task<ServerResult<decimal>> GetBalanceAsync(Account account);

    Task<ServerResult> RejectAsync(Account account, PhoneNumber phoneNumber);

    Task<ServerResult> SuccessAsync(Account account, PhoneNumber phoneNumber);

    Task<ServerResult<string>> GetStatusAsync(Account account, PhoneNumber phoneNumber);
}
