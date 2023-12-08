using System.Diagnostics.CodeAnalysis;

namespace PhoneFinder.Domain;

internal class ServerResult<T> : ServerResult
{
    public ServerResult(T result)
        : this(result, null)
    {
    }

    public ServerResult(Error error)
        : this(default, error)
    {
    }

    private ServerResult(T? result, Error? error)
        : base(error)
    {
        Result = result;
    }

    public T? Result { get; }
}

internal class ServerResult
{
    private static ServerResult? _successResult;
    public static ServerResult SuccessResult => _successResult ??= new ServerResult(null);

    public ServerResult(Error? error)
    {
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => Error is null;

    public Error? Error { get; }
}
