namespace Monads;

/// <summary>
/// Record type to hold validation result status and errors.
/// This represents a validation monad pattern for collecting multiple validation errors.
/// </summary>
public record ValidationResult(bool IsValid, string[] Errors)
{
    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    public static ValidationResult Success()
        => new(true, []);

    /// <summary>
    /// Creates a failed validation result with one or more error messages.
    /// </summary>
    public static ValidationResult Failure(params string[] errors)
        => new(false, errors);
}