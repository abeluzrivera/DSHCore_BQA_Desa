namespace SDH.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string ErrorMessage { get; protected set; } = string.Empty;

        public static Result Success() => new() { IsSuccess = true };
        public static Result Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    }

    public class Result<T> : Result
    {
        public T? Value { get; private set; }

        public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
        public new static Result<T> Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}