namespace Monads;

public static class ScenarioValidation
{
    public static void RunScenario()
    {
        // Sample validations on an initial value of 41 (this will fail)
        var validationResult1 = RunValidations(41, IsNegative, IsEven, LessThan(10));
        PrintValidationResult(41, validationResult1);

        Console.WriteLine();

        // Sample validations on an initial value of 15 (this will pass)
        var validationResult2 = RunValidations(15, IsPositive, LessThan(20));
        PrintValidationResult(15, validationResult2);

        Console.WriteLine();

        // Sample validations on a string value (this will pass)
        var stringValidationResult = RunValidations("Hello", NotNullOrEmpty, MaxLength(10));
        PrintValidationResult("Hello", stringValidationResult);
    }
        
    // Function to print the validation result, including the input value, success status, and any error messages
    static void PrintValidationResult<T>(T input, ValidationResult result)
    {
        Console.WriteLine($"Input: {input}");
        if (result.IsValid)
        {
            Console.WriteLine("Validation passed.");
        }
        else
        {
            Console.WriteLine("Validation failed with errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }
        
    // Validation to check if a number is positive
    static ValidationResult IsPositive(int x)
        => x > 0
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not positive.");
    
    static ValidationResult IsNegative(int x)
        => x < 0
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not negative.");

    // Validation to check if a number is even
    static ValidationResult IsEven(int x)
        => x % 2 == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not even.");

    // Parametrized validation to check if a number is less than a specified value
    static Func<int, ValidationResult> LessThan(int max)
        => x => x < max
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not less than {max}.");

    // Validation to check if a string is not null or empty
    static ValidationResult NotNullOrEmpty(string str)
        => !string.IsNullOrEmpty(str)
            ? ValidationResult.Success()
            : ValidationResult.Failure("String is null or empty.");

    // Parametrized validation to check if a string's length is less than or equal to a specified value
    static Func<string, ValidationResult> MaxLength(int maxLength)
        => str => str.Length <= maxLength
            ? ValidationResult.Success()
            : ValidationResult.Failure($"String length exceeds {maxLength} characters.");

    // Function to run multiple validations on an input value
    static ValidationResult RunValidations<T>(T input, params Func<T, ValidationResult>[] validations)
    {
        var errors = new List<string>();
        foreach (var validate in validations)
        {
            var validationResult = validate(input);
            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors);
            }
        }

        return errors.Count > 0
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }
}

// Record type to hold validation result status and errors
internal record ValidationResult(bool IsValid, string[] Errors)
{
    // Static method to create a successful validation result
    public static ValidationResult Success()
        => new(true, []);

    // Static method to create a failed validation result with error messages
    public static ValidationResult Failure(params string[] errors)
        => new(false, errors);
}