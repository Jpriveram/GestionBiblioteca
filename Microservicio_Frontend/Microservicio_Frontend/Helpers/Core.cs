namespace Frontend.Helpers;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public Error(string code, string message) { Code = code; Message = message; }
}

public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; protected set; }
    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(Error error) => new() { Error = error };
    protected Result() { }
}

public class Result<T> : Result
{
    public T? Value { get; private set; }
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public new static Result<T> Failure(Error error) => new() { Error = error };
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Bibliotecario = "Bibliotecario";
    public const string Lector = "Lector";
}
