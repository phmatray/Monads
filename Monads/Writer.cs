namespace Monads;

/// <summary>
/// Generic Writer monad for accumulating logs/outputs during computations.
/// Implements the Writer monad pattern from functional programming.
/// </summary>
/// <typeparam name="T">The type of the wrapped value</typeparam>
/// <typeparam name="TLog">The type of the log (must implement IEnumerable for concatenation)</typeparam>
public record Writer<T, TLog>
{
    public T Value { get; }
    public TLog Log { get; }

    public Writer(T value, TLog log)
    {
        Value = value;
        Log = log;
    }

    /// <summary>
    /// Creates a Writer with a value and empty log.
    /// Also known as 'return' or 'pure' in monad terminology.
    /// </summary>
    public static Writer<T, TLog> Return(T value, TLog emptyLog)
        => new(value, emptyLog);

    /// <summary>
    /// Maps a function over the value while preserving the log.
    /// Also known as 'fmap' or 'map' in functional programming.
    /// </summary>
    public Writer<TResult, TLog> Map<TResult>(Func<T, TResult> mapper)
        => new(mapper(Value), Log);

    /// <summary>
    /// Monadic bind operation (flatMap).
    /// Chains Writer computations and combines their logs.
    /// </summary>
    public Writer<TResult, TLog> Bind<TResult>(Func<T, Writer<TResult, TLog>> binder, Func<TLog, TLog, TLog> combiner)
    {
        var result = binder(Value);
        return new Writer<TResult, TLog>(result.Value, combiner(Log, result.Log));
    }

    /// <summary>
    /// LINQ Select support (synonym for Map).
    /// Enables query syntax: select x
    /// </summary>
    public Writer<TResult, TLog> Select<TResult>(Func<T, TResult> selector)
        => Map(selector);

    /// <summary>
    /// LINQ SelectMany support (synonym for Bind).
    /// Enables query syntax: from x in ... from y in ...
    /// </summary>
    public Writer<TResult, TLog> SelectMany<TIntermediate, TResult>(
        Func<T, Writer<TIntermediate, TLog>> selector,
        Func<T, TIntermediate, TResult> projector,
        Func<TLog, TLog, TLog> combiner)
    {
        var intermediate = selector(Value);
        var result = projector(Value, intermediate.Value);
        return new Writer<TResult, TLog>(result, combiner(Log, intermediate.Log));
    }

    /// <summary>
    /// Deconstructs the Writer into its value and log components.
    /// </summary>
    public void Deconstruct(out T value, out TLog log)
    {
        value = Value;
        log = Log;
    }
}

/// <summary>
/// Specialized Writer monad for string logs.
/// Most common use case with convenient methods.
/// </summary>
public record Writer<T> : Writer<T, IReadOnlyList<string>>
{
    public Writer(T value, IReadOnlyList<string> log) : base(value, log) { }

    /// <summary>
    /// Creates a Writer with a value and empty log.
    /// </summary>
    public static Writer<T> Return(T value)
        => new(value, Array.Empty<string>());

    /// <summary>
    /// Creates a Writer with a value and a single log entry.
    /// </summary>
    public static Writer<T> Create(T value, string logEntry)
        => new(value, new[] { logEntry });

    /// <summary>
    /// Creates a Writer with a value and multiple log entries.
    /// </summary>
    public static Writer<T> Create(T value, params string[] logEntries)
        => new(value, logEntries);

    /// <summary>
    /// Maps a function over the value while preserving the log.
    /// </summary>
    public new Writer<TResult> Map<TResult>(Func<T, TResult> mapper)
        => new(mapper(Value), Log);

    /// <summary>
    /// Monadic bind operation with automatic log concatenation.
    /// </summary>
    public Writer<TResult> Bind<TResult>(Func<T, Writer<TResult>> binder)
    {
        var result = binder(Value);
        return new Writer<TResult>(result.Value, Log.Concat(result.Log).ToArray());
    }

    /// <summary>
    /// LINQ Select support.
    /// </summary>
    public new Writer<TResult> Select<TResult>(Func<T, TResult> selector)
        => Map(selector);

    /// <summary>
    /// LINQ SelectMany support for query syntax.
    /// </summary>
    public Writer<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Writer<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> projector)
    {
        var intermediate = selector(Value);
        var result = projector(Value, intermediate.Value);
        var combinedLog = Log.Concat(intermediate.Log).ToArray();
        return new Writer<TResult>(result, combinedLog);
    }
}