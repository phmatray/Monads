using Monads;

namespace Monads.Tests;

public class ValidationMonadTests
{
    // Helper validation functions
    private static ValidationResult IsPositive(int x)
        => x > 0
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not positive.");

    private static ValidationResult IsNegative(int x)
        => x < 0
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not negative.");

    private static ValidationResult IsEven(int x)
        => x % 2 == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not even.");

    private static Func<int, ValidationResult> LessThan(int max)
        => x => x < max
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not less than {max}.");

    private static Func<int, ValidationResult> GreaterThan(int min)
        => x => x > min
            ? ValidationResult.Success()
            : ValidationResult.Failure($"{x} is not greater than {min}.");

    private static ValidationResult NotNullOrEmpty(string str)
        => !string.IsNullOrEmpty(str)
            ? ValidationResult.Success()
            : ValidationResult.Failure("String is null or empty.");

    private static Func<string, ValidationResult> MaxLength(int maxLength)
        => str => str.Length <= maxLength
            ? ValidationResult.Success()
            : ValidationResult.Failure($"String length exceeds {maxLength} characters.");

    private static Func<string, ValidationResult> MinLength(int minLength)
        => str => str.Length >= minLength
            ? ValidationResult.Success()
            : ValidationResult.Failure($"String length is less than {minLength} characters.");

    private static ValidationResult RunValidations<T>(T input, params Func<T, ValidationResult>[] validations)
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

    [Fact]
    public void ValidationResult_Success_ShouldHaveEmptyErrorsAndBeValid()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithSingleError_ShouldContainError()
    {
        // Act
        var result = ValidationResult.Failure("Error message");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(1, result.Errors.Length);
        Assert.Equal("Error message", result.Errors[0]);
    }

    [Fact]
    public void ValidationResult_Failure_WithMultipleErrors_ShouldContainAllErrors()
    {
        // Act
        var result = ValidationResult.Failure("Error 1", "Error 2", "Error 3");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Length);
        Assert.Equal("Error 1", result.Errors[0]);
        Assert.Equal("Error 2", result.Errors[1]);
        Assert.Equal("Error 3", result.Errors[2]);
    }

    [Fact]
    public void IsPositive_WithPositiveNumber_ShouldSucceed()
    {
        // Act
        var result = IsPositive(10);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsPositive_WithNegativeNumber_ShouldFail()
    {
        // Act
        var result = IsPositive(-5);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("-5 is not positive.", result.Errors[0]);
    }

    [Fact]
    public void IsPositive_WithZero_ShouldFail()
    {
        // Act
        var result = IsPositive(0);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("0 is not positive.", result.Errors[0]);
    }

    [Fact]
    public void IsNegative_WithNegativeNumber_ShouldSucceed()
    {
        // Act
        var result = IsNegative(-10);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsNegative_WithPositiveNumber_ShouldFail()
    {
        // Act
        var result = IsNegative(5);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("5 is not negative.", result.Errors[0]);
    }

    [Fact]
    public void IsEven_WithEvenNumber_ShouldSucceed()
    {
        // Act
        var result = IsEven(4);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsEven_WithOddNumber_ShouldFail()
    {
        // Act
        var result = IsEven(7);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("7 is not even.", result.Errors[0]);
    }

    [Fact]
    public void LessThan_WithValueLessThanMax_ShouldSucceed()
    {
        // Arrange
        var validate = LessThan(10);

        // Act
        var result = validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void LessThan_WithValueGreaterThanMax_ShouldFail()
    {
        // Arrange
        var validate = LessThan(10);

        // Act
        var result = validate(15);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("15 is not less than 10.", result.Errors[0]);
    }

    [Fact]
    public void NotNullOrEmpty_WithValidString_ShouldSucceed()
    {
        // Act
        var result = NotNullOrEmpty("Hello");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NotNullOrEmpty_WithEmptyString_ShouldFail()
    {
        // Act
        var result = NotNullOrEmpty("");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("String is null or empty.", result.Errors[0]);
    }

    [Fact]
    public void MaxLength_WithStringWithinLimit_ShouldSucceed()
    {
        // Arrange
        var validate = MaxLength(10);

        // Act
        var result = validate("Hello");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithStringExceedingLimit_ShouldFail()
    {
        // Arrange
        var validate = MaxLength(5);

        // Act
        var result = validate("HelloWorld");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("String length exceeds 5 characters.", result.Errors[0]);
    }

    [Fact]
    public void RunValidations_WithAllPassingValidations_ShouldSucceed()
    {
        // Act
        var result = RunValidations(10, IsPositive, IsEven, LessThan(20));

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void RunValidations_WithSingleFailingValidation_ShouldFail()
    {
        // Act
        var result = RunValidations(41, IsNegative, IsEven, LessThan(10));

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Length);
        Assert.Equal("41 is not negative.", result.Errors[0]);
        Assert.Equal("41 is not even.", result.Errors[1]);
        Assert.Equal("41 is not less than 10.", result.Errors[2]);
    }

    [Fact]
    public void RunValidations_WithMultipleFailingValidations_ShouldCollectAllErrors()
    {
        // Act
        var result = RunValidations(-5, IsPositive, IsEven);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Length);
        Assert.Equal("-5 is not positive.", result.Errors[0]);
        Assert.Equal("-5 is not even.", result.Errors[1]);
    }

    [Fact]
    public void RunValidations_WithNoValidations_ShouldSucceed()
    {
        // Act
        var result = RunValidations(42);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void RunValidations_WithStringValidations_ShouldWork()
    {
        // Act
        var result = RunValidations("Hello", NotNullOrEmpty, MaxLength(10));

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RunValidations_WithComplexValidationChain_ShouldCollectAllErrors()
    {
        // Act
        var result = RunValidations(100, IsPositive, IsEven, LessThan(50), GreaterThan(10));

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(1, result.Errors.Length);
        Assert.Equal("100 is not less than 50.", result.Errors[0]);
    }

    [Fact]
    public void RunValidations_WithGenericType_ShouldSupportAnyType()
    {
        // Arrange
        var validateLength = (List<int> list) =>
            list.Count > 0
                ? ValidationResult.Success()
                : ValidationResult.Failure("List is empty.");

        var validateMaxSize = (List<int> list) =>
            list.Count <= 10
                ? ValidationResult.Success()
                : ValidationResult.Failure("List exceeds maximum size.");

        // Act
        var result = RunValidations(new List<int> { 1, 2, 3 }, validateLength, validateMaxSize);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidationResult_ShouldBeImmutableRecord()
    {
        // Arrange
        var original = ValidationResult.Failure("Error");

        // Act
        var modified = original with { IsValid = true };

        // Assert
        Assert.False(original.IsValid);
        Assert.True(modified.IsValid);
    }
}
