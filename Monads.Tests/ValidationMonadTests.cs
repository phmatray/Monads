using Monads;

namespace Monads.Tests;

[TestFixture]
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

    [Test]
    public void ValidationResult_Success_ShouldHaveEmptyErrorsAndBeValid()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void ValidationResult_Failure_WithSingleError_ShouldContainError()
    {
        // Act
        var result = ValidationResult.Failure("Error message");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(1));
        Assert.That(result.Errors[0], Is.EqualTo("Error message"));
    }

    [Test]
    public void ValidationResult_Failure_WithMultipleErrors_ShouldContainAllErrors()
    {
        // Act
        var result = ValidationResult.Failure("Error 1", "Error 2", "Error 3");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(3));
        Assert.That(result.Errors[0], Is.EqualTo("Error 1"));
        Assert.That(result.Errors[1], Is.EqualTo("Error 2"));
        Assert.That(result.Errors[2], Is.EqualTo("Error 3"));
    }

    [Test]
    public void IsPositive_WithPositiveNumber_ShouldSucceed()
    {
        // Act
        var result = IsPositive(10);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void IsPositive_WithNegativeNumber_ShouldFail()
    {
        // Act
        var result = IsPositive(-5);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("-5 is not positive."));
    }

    [Test]
    public void IsPositive_WithZero_ShouldFail()
    {
        // Act
        var result = IsPositive(0);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("0 is not positive."));
    }

    [Test]
    public void IsNegative_WithNegativeNumber_ShouldSucceed()
    {
        // Act
        var result = IsNegative(-10);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void IsNegative_WithPositiveNumber_ShouldFail()
    {
        // Act
        var result = IsNegative(5);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("5 is not negative."));
    }

    [Test]
    public void IsEven_WithEvenNumber_ShouldSucceed()
    {
        // Act
        var result = IsEven(4);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void IsEven_WithOddNumber_ShouldFail()
    {
        // Act
        var result = IsEven(7);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("7 is not even."));
    }

    [Test]
    public void LessThan_WithValueLessThanMax_ShouldSucceed()
    {
        // Arrange
        var validate = LessThan(10);

        // Act
        var result = validate(5);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void LessThan_WithValueGreaterThanMax_ShouldFail()
    {
        // Arrange
        var validate = LessThan(10);

        // Act
        var result = validate(15);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("15 is not less than 10."));
    }

    [Test]
    public void NotNullOrEmpty_WithValidString_ShouldSucceed()
    {
        // Act
        var result = NotNullOrEmpty("Hello");

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void NotNullOrEmpty_WithEmptyString_ShouldFail()
    {
        // Act
        var result = NotNullOrEmpty("");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("String is null or empty."));
    }

    [Test]
    public void MaxLength_WithStringWithinLimit_ShouldSucceed()
    {
        // Arrange
        var validate = MaxLength(10);

        // Act
        var result = validate("Hello");

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void MaxLength_WithStringExceedingLimit_ShouldFail()
    {
        // Arrange
        var validate = MaxLength(5);

        // Act
        var result = validate("HelloWorld");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0], Is.EqualTo("String length exceeds 5 characters."));
    }

    [Test]
    public void RunValidations_WithAllPassingValidations_ShouldSucceed()
    {
        // Act
        var result = RunValidations(10, IsPositive, IsEven, LessThan(20));

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void RunValidations_WithSingleFailingValidation_ShouldFail()
    {
        // Act
        var result = RunValidations(41, IsNegative, IsEven, LessThan(10));

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(3));
        Assert.That(result.Errors[0], Is.EqualTo("41 is not negative."));
        Assert.That(result.Errors[1], Is.EqualTo("41 is not even."));
        Assert.That(result.Errors[2], Is.EqualTo("41 is not less than 10."));
    }

    [Test]
    public void RunValidations_WithMultipleFailingValidations_ShouldCollectAllErrors()
    {
        // Act
        var result = RunValidations(-5, IsPositive, IsEven);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(2));
        Assert.That(result.Errors[0], Is.EqualTo("-5 is not positive."));
        Assert.That(result.Errors[1], Is.EqualTo("-5 is not even."));
    }

    [Test]
    public void RunValidations_WithNoValidations_ShouldSucceed()
    {
        // Act
        var result = RunValidations(42);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void RunValidations_WithStringValidations_ShouldWork()
    {
        // Act
        var result = RunValidations("Hello", NotNullOrEmpty, MaxLength(10));

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void RunValidations_WithComplexValidationChain_ShouldCollectAllErrors()
    {
        // Act
        var result = RunValidations(100, IsPositive, IsEven, LessThan(50), GreaterThan(10));

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Length.EqualTo(1));
        Assert.That(result.Errors[0], Is.EqualTo("100 is not less than 50."));
    }

    [Test]
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
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidationResult_ShouldBeImmutableRecord()
    {
        // Arrange
        var original = ValidationResult.Failure("Error");

        // Act
        var modified = original with { IsValid = true };

        // Assert
        Assert.That(original.IsValid, Is.False);
        Assert.That(modified.IsValid, Is.True);
    }
}
