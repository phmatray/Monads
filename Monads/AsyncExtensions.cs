namespace Monads;

/// <summary>
/// Extension methods for combining monads with async/await.
/// </summary>
public static class AsyncExtensions
{
    #region Option Async Extensions

    /// <summary>
    /// Maps an async function over an Option.
    /// </summary>
    public static async Task<Option<TResult>> MapAsync<T, TResult>(
        this Option<T> option,
        Func<T, Task<TResult>> asyncMapper)
        => await option.Match(
            onSome: async value => Option<TResult>.Some(await asyncMapper(value)),
            onNone: () => Task.FromResult(Option<TResult>.None()));

    /// <summary>
    /// Binds an async function to an Option.
    /// </summary>
    public static async Task<Option<TResult>> BindAsync<T, TResult>(
        this Option<T> option,
        Func<T, Task<Option<TResult>>> asyncBinder)
        => await option.Match(
            onSome: asyncBinder,
            onNone: () => Task.FromResult(Option<TResult>.None()));

    /// <summary>
    /// Maps a function over a Task<Option<T>>.
    /// </summary>
    public static async Task<Option<TResult>> Map<T, TResult>(
        this Task<Option<T>> optionTask,
        Func<T, TResult> mapper)
    {
        var option = await optionTask;
        return option.Map(mapper);
    }

    /// <summary>
    /// Binds a function to a Task<Option<T>>.
    /// </summary>
    public static async Task<Option<TResult>> Bind<T, TResult>(
        this Task<Option<T>> optionTask,
        Func<T, Option<TResult>> binder)
    {
        var option = await optionTask;
        return option.Bind(binder);
    }

    /// <summary>
    /// Binds an async function to a Task<Option<T>>.
    /// </summary>
    public static async Task<Option<TResult>> BindAsync<T, TResult>(
        this Task<Option<T>> optionTask,
        Func<T, Task<Option<TResult>>> asyncBinder)
    {
        var option = await optionTask;
        return await option.BindAsync(asyncBinder);
    }

    #endregion

    #region Result Async Extensions

    /// <summary>
    /// Maps an async function over a Result's success value.
    /// </summary>
    public static async Task<Result<TResult, TError>> MapAsync<T, TResult, TError>(
        this Result<T, TError> result,
        Func<T, Task<TResult>> asyncMapper)
        => await result.Match(
            onSuccess: async value => Result<TResult, TError>.Success(await asyncMapper(value)),
            onFailure: error => Task.FromResult(Result<TResult, TError>.Failure(error)));

    /// <summary>
    /// Binds an async function to a Result.
    /// </summary>
    public static async Task<Result<TResult, TError>> BindAsync<T, TResult, TError>(
        this Result<T, TError> result,
        Func<T, Task<Result<TResult, TError>>> asyncBinder)
        => await result.Match(
            onSuccess: asyncBinder,
            onFailure: error => Task.FromResult(Result<TResult, TError>.Failure(error)));

    /// <summary>
    /// Maps a function over a Task<Result<T, TError>>.
    /// </summary>
    public static async Task<Result<TResult, TError>> Map<T, TResult, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, TResult> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Binds a function to a Task<Result<T, TError>>.
    /// </summary>
    public static async Task<Result<TResult, TError>> Bind<T, TResult, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Result<TResult, TError>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    /// <summary>
    /// Binds an async function to a Task<Result<T, TError>>.
    /// </summary>
    public static async Task<Result<TResult, TError>> BindAsync<T, TResult, TError>(
        this Task<Result<T, TError>> resultTask,
        Func<T, Task<Result<TResult, TError>>> asyncBinder)
    {
        var result = await resultTask;
        return await result.BindAsync(asyncBinder);
    }

    /// <summary>
    /// Maps an async function over a Result<T> (with string errors).
    /// </summary>
    public static async Task<Result<TResult>> MapAsync<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> asyncMapper)
        => await result.Match(
            onSuccess: async value => Result<TResult>.Success(await asyncMapper(value)),
            onFailure: error => Task.FromResult(Result<TResult>.Failure(error)));

    /// <summary>
    /// Binds an async function to a Result<T> (with string errors).
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(
        this Result<T> result,
        Func<T, Task<Result<TResult>>> asyncBinder)
        => await result.Match(
            onSuccess: asyncBinder,
            onFailure: error => Task.FromResult(Result<TResult>.Failure(error)));

    /// <summary>
    /// Creates a Result from an async try-catch operation.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> asyncOperation)
    {
        try
        {
            var value = await asyncOperation();
            return Result<T>.Success(value);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }

    #endregion

    #region Writer Async Extensions

    /// <summary>
    /// Maps an async function over a Writer's value.
    /// </summary>
    public static async Task<Writer<TResult>> MapAsync<T, TResult>(
        this Writer<T> writer,
        Func<T, Task<TResult>> asyncMapper)
    {
        var result = await asyncMapper(writer.Value);
        return new Writer<TResult>(result, writer.Log);
    }

    /// <summary>
    /// Binds an async function to a Writer.
    /// </summary>
    public static async Task<Writer<TResult>> BindAsync<T, TResult>(
        this Writer<T> writer,
        Func<T, Task<Writer<TResult>>> asyncBinder)
    {
        var result = await asyncBinder(writer.Value);
        var combinedLog = writer.Log.Concat(result.Log).ToArray();
        return new Writer<TResult>(result.Value, combinedLog);
    }

    /// <summary>
    /// Maps a function over a Task<Writer<T>>.
    /// </summary>
    public static async Task<Writer<TResult>> Map<T, TResult>(
        this Task<Writer<T>> writerTask,
        Func<T, TResult> mapper)
    {
        var writer = await writerTask;
        return writer.Map(mapper);
    }

    /// <summary>
    /// Binds a function to a Task<Writer<T>>.
    /// </summary>
    public static async Task<Writer<TResult>> Bind<T, TResult>(
        this Task<Writer<T>> writerTask,
        Func<T, Writer<TResult>> binder)
    {
        var writer = await writerTask;
        return writer.Bind(binder);
    }

    /// <summary>
    /// Binds an async function to a Task<Writer<T>>.
    /// </summary>
    public static async Task<Writer<TResult>> BindAsync<T, TResult>(
        this Task<Writer<T>> writerTask,
        Func<T, Task<Writer<TResult>>> asyncBinder)
    {
        var writer = await writerTask;
        return await writer.BindAsync(asyncBinder);
    }

    #endregion

    #region Traverse and Sequence

    /// <summary>
    /// Traverses a collection with an async function that returns Options.
    /// Returns Some if all succeed, None if any fails.
    /// </summary>
    public static async Task<Option<IReadOnlyList<TResult>>> TraverseAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Task<Option<TResult>>> asyncFunc)
    {
        var results = new List<TResult>();
        foreach (var item in source)
        {
            var result = await asyncFunc(item);
            if (result.IsNone)
                return Option<IReadOnlyList<TResult>>.None();

            result.Match(
                onSome: value => results.Add(value),
                onNone: () => { });
        }
        return Option<IReadOnlyList<TResult>>.Some(results);
    }

    /// <summary>
    /// Traverses a collection with an async function that returns Results.
    /// Returns Success with all values, or the first Failure encountered.
    /// </summary>
    public static async Task<Result<IReadOnlyList<TResult>, TError>> TraverseAsync<T, TResult, TError>(
        this IEnumerable<T> source,
        Func<T, Task<Result<TResult, TError>>> asyncFunc)
    {
        var results = new List<TResult>();
        foreach (var item in source)
        {
            var result = await asyncFunc(item);
            if (result.IsFailure)
            {
                return result.Match(
                    onSuccess: _ => Result<IReadOnlyList<TResult>, TError>.Success(results),
                    onFailure: Result<IReadOnlyList<TResult>, TError>.Failure);
            }

            result.Match(
                onSuccess: value => results.Add(value),
                onFailure: _ => { });
        }
        return Result<IReadOnlyList<TResult>, TError>.Success(results);
    }

    /// <summary>
    /// Sequences a collection of async Options.
    /// Returns Some with all values if all are Some, otherwise None.
    /// </summary>
    public static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        this IEnumerable<Task<Option<T>>> source)
    {
        var results = new List<T>();
        foreach (var optionTask in source)
        {
            var option = await optionTask;
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            option.Match(
                onSome: value => results.Add(value),
                onNone: () => { });
        }
        return Option<IReadOnlyList<T>>.Some(results);
    }

    /// <summary>
    /// Sequences a collection of async Results.
    /// Returns Success with all values if all succeed, otherwise the first Failure.
    /// </summary>
    public static async Task<Result<IReadOnlyList<T>, TError>> SequenceAsync<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> source)
    {
        var results = new List<T>();
        foreach (var resultTask in source)
        {
            var result = await resultTask;
            if (result.IsFailure)
            {
                return result.Match(
                    onSuccess: _ => Result<IReadOnlyList<T>, TError>.Success(results),
                    onFailure: Result<IReadOnlyList<T>, TError>.Failure);
            }

            result.Match(
                onSuccess: value => results.Add(value),
                onFailure: _ => { });
        }
        return Result<IReadOnlyList<T>, TError>.Success(results);
    }

    #endregion
}
