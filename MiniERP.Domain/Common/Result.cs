namespace MiniERP.Domain.Common;

public sealed record Result(
    bool IsSuccess,
    string Message,
    IEnumerable<string>? Errors = null)
{
    public static Result Success(string message) =>
        new(true, message);

    public static Result Failure(string message, IEnumerable<string>? errors = null) =>
        new(false, message, errors);
}