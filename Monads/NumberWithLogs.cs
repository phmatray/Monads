namespace Monads;

/// <summary>
/// Record type to hold an integer result and associated logs.
/// This represents a Writer monad pattern for accumulating logs during transformations.
/// </summary>
public record NumberWithLogs(int Result, string[] Logs);