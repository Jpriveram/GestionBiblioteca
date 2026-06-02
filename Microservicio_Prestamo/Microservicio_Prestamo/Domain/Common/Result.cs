namespace Microservicio_Prestamo.Domain.Common;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public Error(string code, string message) { Code = code; Message = message; }
    public override string ToString() => $"{Code}: {Message}";
}

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    private Result(bool isSuccess, Error? error) { IsSuccess = isSuccess; Error = error; }
    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(true, value, null);
    public static Result<T> Failure<T>(Error error) => new(false, default, error);
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    public Result(bool isSuccess, T? value, Error? error) { IsSuccess = isSuccess; Value = value; Error = error; }
    public static implicit operator Result<T>(T value) => new(true, value, null);
    public static implicit operator Result<T>(Error error) => new(false, default, error);
}
