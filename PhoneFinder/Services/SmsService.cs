using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PhoneFinder.Domain;

namespace PhoneFinder.Services;

internal class SmsService : ISmsService
{
    private ILogger _logger;
    private const string RussianCountryCode = "0";
    private const string GolosZaServiceCode = "js";

    public string BaseUrl => "https://365sms.ru";

    public SmsService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(typeof(SmsService));
    }

    private async Task<string> GetStringAsync(Uri uri)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(uri);
        _logger.LogInformation(@"Received response: {0}", response);
        return response;
    }

    public async Task<ServerResult<PhoneNumber>> GetPhoneNumberAsync(Account account)
    {
        var uriBuilder = CreateBuilder($"?api_key={account.Token}&action=getNumber&service={GolosZaServiceCode}&country={RussianCountryCode}");

        var response = await GetStringAsync(uriBuilder.Uri);

        if (TryHandleCommonErrors(response, out var error))
            return new ServerResult<PhoneNumber>(error);

        switch (response)
        {
            case "NO_NUMBERS":
                return new ServerResult<PhoneNumber>(
                    new Error(ErrorCode.NO_NUMBERS, "Нет номеров с заданными параметрами, попробуйте позже, или поменяйте оператора, страну"));
            case "NO_BALANCE":
                return new ServerResult<PhoneNumber>(
                    new Error(ErrorCode.NO_BALANCE, "Закончились деньги на аккаунте"));
            case "WRONG_SERVICE":
                return new ServerResult<PhoneNumber>(
                    new Error(ErrorCode.WRONG_SERVICE, "Неверный идентификатор сервиса"));
            default: //ACCESS_NUMBER:ID:NUMBER
                var parts = response.Split(':');

                if (parts.Length != 3)
                    return new ServerResult<PhoneNumber>(CreateUnknownError(response));

                var code = parts[1];
                var number = parts[2];
                return new ServerResult<PhoneNumber>(new PhoneNumber(Id: code, number[1..4], number[4..]));
        }
    }

    public async Task<ServerResult<decimal>> GetBalanceAsync(Account account)
    {
        var uriBuilder = CreateBuilder($"?api_key={account.Token}&action=getBalance");
        var response = await GetStringAsync(uriBuilder.Uri);

        if (TryHandleCommonErrors(response, out var error))
            return new ServerResult<decimal>(error);

        var balanceString = response.Split(':')[1];
        return new ServerResult<decimal>(decimal.Parse(balanceString, CultureInfo.InvariantCulture));
    }

    public async Task<ServerResult> RejectAsync(Account account, PhoneNumber phoneNumber)
    {
        var uriBuilder = CreateBuilder($"?api_key={account.Token}&action=setStatus&status=8&id={phoneNumber.Id}");

        var response = await GetStringAsync(uriBuilder.Uri);

        if (TryHandleCommonErrors(response, out var error))
            return new ServerResult(error);

        return response == "ACCESS_CANCEL"
            ? ServerResult.SuccessResult
            : new ServerResult(CreateUnknownError(response));
    }

    public async Task<ServerResult> SuccessAsync(Account account, PhoneNumber phoneNumber)
    {
        var uriBuilder = CreateBuilder($"?api_key={account.Token}&action=setStatus&status=6&id={phoneNumber.Id}");
        var response = await GetStringAsync(uriBuilder.Uri);

        if (TryHandleCommonErrors(response, out var error))
            return new ServerResult(error);

        return response == "ACCESS_ACTIVATION"
            ? ServerResult.SuccessResult
            : new ServerResult(CreateUnknownError(response));
    }

    public async Task<ServerResult<string>> GetStatusAsync(Account account, PhoneNumber phoneNumber)
    {
        var uriBuilder = CreateBuilder($"?api_key={account.Token}&action=getStatus&id={phoneNumber.Id}");
        var response = await GetStringAsync(uriBuilder.Uri);

        if (TryHandleCommonErrors(response, out var error))
            return new ServerResult<string>(error);

        switch (response)
        {
            case "STATUS_WAIT_CODE":
                return new ServerResult<string>(
                    new Error(ErrorCode.STATUS_WAIT_CODE, "Ожидаем прихода СМС"));
            case "STATUS_CANCEL":
                return new ServerResult<string>(
                    new Error(ErrorCode.STATUS_CANCEL, "Активация отменена"));

            default: //STATUS_OK:CODE
                var parts = response.Split(':');

                return parts.Length != 2
                    ? new ServerResult<string>(CreateUnknownError(response))
                    : new ServerResult<string>(parts[1]);
        }
    }

    private UriBuilder CreateBuilder(string query)
    {
        return new UriBuilder(BaseUrl) { Path = "/stubs/handler_api.php", Query = query };
    }

    private static bool TryHandleCommonErrors(string response, [NotNullWhen(true)] out Error? error)
    {
        switch (response)
        {
            case "BAD_ACTION":
                {
                    error = new Error(ErrorCode.BAD_ACTION, "Неправильное формирование запроса");
                    return true;
                }
            case "BAD_SERVICE":
                {
                    error = new Error(ErrorCode.BAD_SERVICE, "Некорректное наименование сервиса");
                    return true;
                }
            case "BAD_KEY":
                {
                    error = new Error(ErrorCode.BAD_KEY, "Неверный API-ключ");
                    return true;
                }
            case "ERROR_SQL":
                {
                    error = new Error(ErrorCode.ERROR_SQL, "Ошибка базы SQL-сервера");
                    return true;
                }
            case "NO_ACTIVATION":
                {
                    error = new Error(ErrorCode.NO_ACTIVATION, "ID активации не существует");
                    return true;
                }
        }

        error = null;
        return false;
    }

    private static Error CreateUnknownError(string response)
    {
        return new Error(ErrorCode.UNKNOWN, $"Неизвестная ошибка: {response}");
    }
}
