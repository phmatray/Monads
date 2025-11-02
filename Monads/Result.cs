namespace Monads;

/// <summary>
/// Result monad for error handling without exceptions (Either pattern).
/// Represents a value that is either a Success or a Failure.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
/// <typeparam name="TError">The type of the error</typeparam>
public abstract record Result<T, TError>
{
    /// <summary>
    /// Indicates whether this result is a success.
    /// </summary>
    public abstract bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether this result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a successful Result.
    /// </summary>
    public static Result<T, TError> Success(T value) => new SuccessResult(value);

    /// <summary>
    /// Creates a failed Result.
    /// </summary>
    public static Result<T, TError> Failure(TError error) => new FailureResult(error);

    /// <summary>
    /// Maps a function over the success value.
    /// </summary>
    public abstract Result<TResult, TError> Map<TResult>(Func<T, TResult> mapper);

    /// <summary>
    /// Maps a function over the error value.
    /// </summary>
    public abstract Result<T, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> mapper);

    /// <summary>
    /// Monadic bind operation (flatMap).
    /// </summary>
    public abstract Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> binder);

    /// <summary>
    /// Executes an action if the result is a success.
    /// </summary>
    public abstract Result<T, TError> Do(Action<T> action);

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public abstract Result<T, TError> DoOnError(Action<TError> action);

    /// <summary>
    /// Executes the appropriate action based on success or failure.
    /// </summary>
    public abstract void Match(Action<T> onSuccess, Action<TError> onFailure);

    /// <summary>
    /// Returns a value based on success or failure.
    /// </summary>
    public abstract TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onFailure);

    /// <summary>
    /// Returns the value if success, otherwise returns the provided default value.
    /// </summary>
    public abstract T GetValueOrDefault(T defaultValue);

    /// <summary>
    /// Returns the value if success, otherwise returns the result of the provided function.
    /// </summary>
    public abstract T GetValueOrDefault(Func<TError, T> defaultValueFactory);

    /// <summary>
    /// Converts failure to success using a recovery function.
    /// </summary>
    public abstract Result<T, TError> OrElse(Func<TError, Result<T, TError>> recovery);

    /// <summary>
    /// LINQ Select support (synonym for Map).
    /// </summary>
    public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector)
        => Map(selector);

    /// <summary>
    /// LINQ SelectMany support (synonym for Bind).
    /// </summary>
    public Result<TResult, TError> SelectMany<TIntermediate, TResult>(
        Func<T, Result<TIntermediate, TError>> selector,
        Func<T, TIntermediate, TResult> projector)
        => Bind(x => selector(x).Map(y => projector(x, y)));

    /// <summary>
    /// LINQ Where support for filtering results.
    /// </summary>
    public Result<T, TError> Where(Func<T, bool> predicate, Func<T, TError> errorFactory)
        => Bind(value => predicate(value) ? Success(value) : Failure(errorFactory(value)));

    /// <summary>
    /// Converts the Result to an Option, discarding the error.
    /// </summary>
    public abstract Option<T> ToOption();

    private sealed record SuccessResult(T Value) : Result<T, TError>
    {
        public override bool IsSuccess => true;

        public override Result<TResult, TError> Map<TResult>(Func<T, TResult> mapper)
            => Result<TResult, TError>.Success(mapper(Value));

        public override Result<T, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> mapper)
            => Result<T, TErrorResult>.Success(Value);

        public override Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> binder)
            => binder(Value);

        public override Result<T, TError> Do(Action<T> action)
        {
            action(Value);
            return this;
        }

        public override Result<T, TError> DoOnError(Action<TError> action)
            => this;

        public override void Match(Action<T> onSuccess, Action<TError> onFailure)
            => onSuccess(Value);

        public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onFailure)
            => onSuccess(Value);

        public override T GetValueOrDefault(T defaultValue)
            => Value;

        public override T GetValueOrDefault(Func<TError, T> defaultValueFactory)
            => Value;

        public override Result<T, TError> OrElse(Func<TError, Result<T, TError>> recovery)
            => this;

        public override Option<T> ToOption()
            => Option<T>.Some(Value);
    }

    private sealed record FailureResult(TError Error) : Result<T, TError>
    {
        public override bool IsSuccess => false;

        public override Result<TResult, TError> Map<TResult>(Func<T, TResult> mapper)
            => Result<TResult, TError>.Failure(Error);

        public override Result<T, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> mapper)
            => Result<T, TErrorResult>.Failure(mapper(Error));

        public override Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> binder)
            => Result<TResult, TError>.Failure(Error);

        public override Result<T, TError> Do(Action<T> action)
            => this;

        public override Result<T, TError> DoOnError(Action<TError> action)
        {
            action(Error);
            return this;
        }

        public override void Match(Action<T> onSuccess, Action<TError> onFailure)
            => onFailure(Error);

        public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onFailure)
            => onFailure(Error);

        public override T GetValueOrDefault(T defaultValue)
            => defaultValue;

        public override T GetValueOrDefault(Func<TError, T> defaultValueFactory)
            => defaultValueFactory(Error);

        public override Result<T, TError> OrElse(Func<TError, Result<T, TError>> recovery)
            => recovery(Error);

        public override Option<T> ToOption()
            => Option<T>.None();
    }
}

/// <summary>
/// Result monad with string errors (most common use case).
/// </summary>
public abstract record Result<T> : Result<T, string>
{
    /// <summary>
    /// Creates a successful Result.
    /// </summary>
    public new static Result<T> Success(T value) => new SuccessResultString(value);

    /// <summary>
    /// Creates a failed Result with an error message.
    /// </summary>
    public new static Result<T> Failure(string error) => new FailureResultString(error);

    /// <summary>
    /// Creates a Result from a try-catch operation.
    /// </summary>
    public static Result<T> Try(Func<T> operation)
    {
        try
        {
            return Success(operation());
        }
        catch (Exception ex)
        {
            return Failure(ex.Message);
        }
    }

    private sealed record SuccessResultString(T Value) : Result<T>
    {
        public override bool IsSuccess => true;

        public override Result<TResult, string> Map<TResult>(Func<T, TResult> mapper)
            => Result<TResult>.Success(mapper(Value));

        public override Result<T, TErrorResult> MapError<TErrorResult>(Func<string, TErrorResult> mapper)
            => Result<T, TErrorResult>.Success(Value);

        public override Result<TResult, string> Bind<TResult>(Func<T, Result<TResult, string>> binder)
            => binder(Value);

        public override Result<T, string> Do(Action<T> action)
        {
            action(Value);
            return this;
        }

        public override Result<T, string> DoOnError(Action<string> action)
            => this;

        public override void Match(Action<T> onSuccess, Action<string> onFailure)
            => onSuccess(Value);

        public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
            => onSuccess(Value);

        public override T GetValueOrDefault(T defaultValue)
            => Value;

        public override T GetValueOrDefault(Func<string, T> defaultValueFactory)
            => Value;

        public override Result<T, string> OrElse(Func<string, Result<T, string>> recovery)
            => this;

        public override Option<T> ToOption()
            => Option<T>.Some(Value);
    }

    private sealed record FailureResultString(string Error) : Result<T>
    {
        public override bool IsSuccess => false;

        public override Result<TResult, string> Map<TResult>(Func<T, TResult> mapper)
            => Result<TResult>.Failure(Error);

        public override Result<T, TErrorResult> MapError<TErrorResult>(Func<string, TErrorResult> mapper)
            => Result<T, TErrorResult>.Failure(mapper(Error));

        public override Result<TResult, string> Bind<TResult>(Func<T, Result<TResult, string>> binder)
            => Result<TResult>.Failure(Error);

        public override Result<T, string> Do(Action<T> action)
            => this;

        public override Result<T, string> DoOnError(Action<string> action)
        {
            action(Error);
            return this;
        }

        public override void Match(Action<T> onSuccess, Action<string> onFailure)
            => onFailure(Error);

        public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
            => onFailure(Error);

        public override T GetValueOrDefault(T defaultValue)
            => defaultValue;

        public override T GetValueOrDefault(Func<string, T> defaultValueFactory)
            => defaultValueFactory(Error);

        public override Result<T, string> OrElse(Func<string, Result<T, string>> recovery)
            => recovery(Error);

        public override Option<T> ToOption()
            => Option<T>.None();
    }
}

/// <summary>
/// Extension methods for Result monad.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Flattens a nested Result.
    /// </summary>
    public static Result<T, TError> Flatten<T, TError>(this Result<Result<T, TError>, TError> result)
        => result.Bind(x => x);

    /// <summary>
    /// Combines two Results using a combining function.
    /// Returns failure if either Result is a failure.
    /// </summary>
    public static Result<TResult, TError> Zip<T1, T2, TResult, TError>(
        this Result<T1, TError> result1,
        Result<T2, TError> result2,
        Func<T1, T2, TResult> combiner)
        => result1.Bind(v1 => result2.Map(v2 => combiner(v1, v2)));

    /// <summary>
    /// Converts an Option to a Result.
    /// </summary>
    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, TError error)
        => option.Match(
            onSome: Result<T, TError>.Success,
            onNone: () => Result<T, TError>.Failure(error));

    /// <summary>
    /// Converts an Option to a Result with string error.
    /// </summary>
    public static Result<T> ToResult<T>(this Option<T> option, string errorMessage)
        => option.Match(
            onSome: Result<T>.Success,
            onNone: () => Result<T>.Failure(errorMessage));
}