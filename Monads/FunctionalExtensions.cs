namespace Monads;

/// <summary>
/// Functional programming utility extensions and composition helpers.
/// </summary>
public static class FunctionalExtensions
{
    #region Function Composition

    /// <summary>
    /// Composes two functions: (f ∘ g)(x) = f(g(x))
    /// </summary>
    public static Func<T, TResult> Compose<T, TIntermediate, TResult>(
        this Func<TIntermediate, TResult> f,
        Func<T, TIntermediate> g)
        => x => f(g(x));

    /// <summary>
    /// Pipes a value through a function.
    /// </summary>
    public static TResult Pipe<T, TResult>(this T value, Func<T, TResult> func)
        => func(value);

    /// <summary>
    /// Applies a function to a value (reverse application).
    /// </summary>
    public static TResult Apply<T, TResult>(this Func<T, TResult> func, T value)
        => func(value);

    #endregion

    #region Currying

    /// <summary>
    /// Curries a function with two parameters.
    /// </summary>
    public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(
        this Func<T1, T2, TResult> func)
        => x => y => func(x, y);

    /// <summary>
    /// Curries a function with three parameters.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(
        this Func<T1, T2, T3, TResult> func)
        => x => y => z => func(x, y, z);

    /// <summary>
    /// Uncurries a curried function with two parameters.
    /// </summary>
    public static Func<T1, T2, TResult> Uncurry<T1, T2, TResult>(
        this Func<T1, Func<T2, TResult>> func)
        => (x, y) => func(x)(y);

    /// <summary>
    /// Partially applies the first argument to a two-parameter function.
    /// </summary>
    public static Func<T2, TResult> Partial<T1, T2, TResult>(
        this Func<T1, T2, TResult> func,
        T1 arg1)
        => arg2 => func(arg1, arg2);

    #endregion

    #region Identity and Const

    /// <summary>
    /// Identity function - returns its input unchanged.
    /// </summary>
    public static T Identity<T>(T value) => value;

    /// <summary>
    /// Const function - always returns the same value regardless of input.
    /// </summary>
    public static Func<T, TResult> Const<T, TResult>(TResult value)
        => _ => value;

    #endregion

    #region Memoization

    /// <summary>
    /// Memoizes a function, caching results for repeated inputs.
    /// </summary>
    public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func)
        where T : notnull
    {
        var cache = new Dictionary<T, TResult>();
        return input =>
        {
            if (cache.TryGetValue(input, out var cachedResult))
                return cachedResult;

            var result = func(input);
            cache[input] = result;
            return result;
        };
    }

    #endregion

    #region Tap (Side Effects)

    /// <summary>
    /// Executes an action on a value and returns the value (for side effects).
    /// Useful for debugging or logging in pipelines.
    /// </summary>
    public static T Tap<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }

    /// <summary>
    /// Executes an async action on a value and returns the value.
    /// </summary>
    public static async Task<T> TapAsync<T>(this T value, Func<T, Task> asyncAction)
    {
        await asyncAction(value);
        return value;
    }

    /// <summary>
    /// Executes an async action on a Task value and returns the value.
    /// </summary>
    public static async Task<T> TapAsync<T>(this Task<T> valueTask, Func<T, Task> asyncAction)
    {
        var value = await valueTask;
        await asyncAction(value);
        return value;
    }

    #endregion

    #region Collection Extensions

    /// <summary>
    /// Sequences a collection of Options.
    /// Returns Some with all values if all are Some, otherwise None.
    /// </summary>
    public static Option<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        var results = new List<T>();
        foreach (var option in options)
        {
            if (option.IsNone)
                return Option<IReadOnlyList<T>>.None();

            option.Match(
                onSome: value => results.Add(value),
                onNone: () => { });
        }
        return Option<IReadOnlyList<T>>.Some(results);
    }

    /// <summary>
    /// Sequences a collection of Results.
    /// Returns Success with all values if all succeed, otherwise the first Failure.
    /// </summary>
    public static Result<IReadOnlyList<T>, TError> Sequence<T, TError>(
        this IEnumerable<Result<T, TError>> results)
    {
        var values = new List<T>();
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result.Match(
                    onSuccess: _ => Result<IReadOnlyList<T>, TError>.Success(values),
                    onFailure: Result<IReadOnlyList<T>, TError>.Failure);
            }

            result.Match(
                onSuccess: value => values.Add(value),
                onFailure: _ => { });
        }
        return Result<IReadOnlyList<T>, TError>.Success(values);
    }

    /// <summary>
    /// Traverses a collection with a function that returns Options.
    /// Returns Some if all succeed, None if any fails.
    /// </summary>
    public static Option<IReadOnlyList<TResult>> Traverse<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Option<TResult>> func)
        => source.Select(func).Sequence();

    /// <summary>
    /// Traverses a collection with a function that returns Results.
    /// Returns Success with all values, or the first Failure encountered.
    /// </summary>
    public static Result<IReadOnlyList<TResult>, TError> Traverse<T, TResult, TError>(
        this IEnumerable<T> source,
        Func<T, Result<TResult, TError>> func)
        => source.Select(func).Sequence();

    /// <summary>
    /// Filters and maps in one operation using Option.
    /// </summary>
    public static IEnumerable<TResult> Choose<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Option<TResult>> chooser)
    {
        foreach (var item in source)
        {
            var option = chooser(item);
            if (option.IsSome)
            {
                option.Match(
                    onSome: value => { },
                    onNone: () => { });

                foreach (var value in option.ToEnumerable())
                    yield return value;
            }
        }
    }

    #endregion

    #region Try-Catch Helpers

    /// <summary>
    /// Executes a function and returns a Result with exception handling.
    /// </summary>
    public static Result<T> Try<T>(Func<T> func)
        => Result<T>.Try(func);

    /// <summary>
    /// Executes an async function and returns a Result with exception handling.
    /// </summary>
    public static Task<Result<T>> TryAsync<T>(Func<Task<T>> asyncFunc)
        => AsyncExtensions.TryAsync(asyncFunc);

    #endregion

    #region Pattern Matching Helpers

    /// <summary>
    /// Provides pattern matching on boolean values.
    /// </summary>
    public static TResult Match<TResult>(this bool value, Func<TResult> onTrue, Func<TResult> onFalse)
        => value ? onTrue() : onFalse();

    /// <summary>
    /// Provides pattern matching on nullable values.
    /// </summary>
    public static TResult Match<T, TResult>(this T? value, Func<T, TResult> onValue, Func<TResult> onNull)
        where T : class
        => value is not null ? onValue(value) : onNull();

    /// <summary>
    /// Provides pattern matching on nullable value types.
    /// </summary>
    public static TResult Match<T, TResult>(this T? value, Func<T, TResult> onValue, Func<TResult> onNull)
        where T : struct
        => value.HasValue ? onValue(value.Value) : onNull();

    #endregion

    #region Kleisli Composition

    /// <summary>
    /// Kleisli composition for Option monad.
    /// Composes two functions that return Options.
    /// </summary>
    public static Func<T, Option<TResult>> ComposeKleisli<T, TIntermediate, TResult>(
        this Func<TIntermediate, Option<TResult>> f,
        Func<T, Option<TIntermediate>> g)
        => x => g(x).Bind(f);

    /// <summary>
    /// Kleisli composition for Result monad.
    /// Composes two functions that return Results.
    /// </summary>
    public static Func<T, Result<TResult, TError>> ComposeKleisli<T, TIntermediate, TResult, TError>(
        this Func<TIntermediate, Result<TResult, TError>> f,
        Func<T, Result<TIntermediate, TError>> g)
        => x => g(x).Bind(f);

    #endregion
}
