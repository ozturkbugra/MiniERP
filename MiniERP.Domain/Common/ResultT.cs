namespace MiniERP.Domain.Common;

public sealed record Result<T>(
    bool IsSuccess,
    string Message,
    T? Data,
    IEnumerable<string>? Errors = null)
{
    // Başarılıysa veriyi (data) de içine koyup yolluyoruz
    public static Result<T> Success(T data, string message) =>
        new(true, message, data);

    public static Result<T> Failure(string message, IEnumerable<string>? errors = null) =>
        new(false, message, default, errors);
}