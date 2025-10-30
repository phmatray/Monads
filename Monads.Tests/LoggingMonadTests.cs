using Monads;

namespace Monads.Tests;

[TestFixture]
public class LoggingMonadTests
{
    // Helper methods to access internal members for testing
    private static NumberWithLogs WrapWithLogs(int x) => new(x, []);

    private static NumberWithLogs AddOne(int x)
        => new(x + 1, [$"Added 1 to {x} to get {x + 1}"]);

    private static NumberWithLogs Square(int x)
        => new(x * x, [$"Squared {x} to get {x * x}"]);

    private static NumberWithLogs MultiplyByThree(int x)
        => new(x * 3, [$"Multiplied {x} by 3 to get {x * 3}"]);

    private static NumberWithLogs RunWithLogs(NumberWithLogs input, Func<int, NumberWithLogs> transform)
    {
        var transformed = transform(input.Result);
        return transformed with
        {
            Logs = input.Logs.Concat(transformed.Logs).ToArray()
        };
    }

    private static NumberWithLogs RunWithLogsMultiple(NumberWithLogs input, params Func<int, NumberWithLogs>[] transforms)
        => transforms.Aggregate(input, RunWithLogs);

    [Test]
    public void WrapWithLogs_ShouldCreateNumberWithLogsWithEmptyLogs()
    {
        // Arrange
        var value = 5;

        // Act
        var result = WrapWithLogs(value);

        // Assert
        Assert.That(result.Result, Is.EqualTo(5));
        Assert.That(result.Logs, Is.Empty);
    }

    [Test]
    public void AddOne_ShouldIncrementValueAndLogOperation()
    {
        // Arrange
        var value = 5;

        // Act
        var result = AddOne(value);

        // Assert
        Assert.That(result.Result, Is.EqualTo(6));
        Assert.That(result.Logs, Has.Length.EqualTo(1));
        Assert.That(result.Logs[0], Is.EqualTo("Added 1 to 5 to get 6"));
    }

    [Test]
    public void Square_ShouldSquareValueAndLogOperation()
    {
        // Arrange
        var value = 4;

        // Act
        var result = Square(value);

        // Assert
        Assert.That(result.Result, Is.EqualTo(16));
        Assert.That(result.Logs, Has.Length.EqualTo(1));
        Assert.That(result.Logs[0], Is.EqualTo("Squared 4 to get 16"));
    }

    [Test]
    public void MultiplyByThree_ShouldMultiplyValueAndLogOperation()
    {
        // Arrange
        var value = 7;

        // Act
        var result = MultiplyByThree(value);

        // Assert
        Assert.That(result.Result, Is.EqualTo(21));
        Assert.That(result.Logs, Has.Length.EqualTo(1));
        Assert.That(result.Logs[0], Is.EqualTo("Multiplied 7 by 3 to get 21"));
    }

    [Test]
    public void RunWithLogs_ShouldCombineLogsFromInputAndTransformation()
    {
        // Arrange
        var input = new NumberWithLogs(5, ["Initial log"]);

        // Act
        var result = RunWithLogs(input, AddOne);

        // Assert
        Assert.That(result.Result, Is.EqualTo(6));
        Assert.That(result.Logs, Has.Length.EqualTo(2));
        Assert.That(result.Logs[0], Is.EqualTo("Initial log"));
        Assert.That(result.Logs[1], Is.EqualTo("Added 1 to 5 to get 6"));
    }

    [Test]
    public void RunWithLogsMultiple_ShouldApplyAllTransformationsInOrder()
    {
        // Arrange
        var input = WrapWithLogs(5);

        // Act
        // 5 -> AddOne -> 6 -> Square -> 36 -> MultiplyByThree -> 108
        var result = RunWithLogsMultiple(input, AddOne, Square, MultiplyByThree);

        // Assert
        Assert.That(result.Result, Is.EqualTo(108));
        Assert.That(result.Logs, Has.Length.EqualTo(3));
        Assert.That(result.Logs[0], Is.EqualTo("Added 1 to 5 to get 6"));
        Assert.That(result.Logs[1], Is.EqualTo("Squared 6 to get 36"));
        Assert.That(result.Logs[2], Is.EqualTo("Multiplied 36 by 3 to get 108"));
    }

    [Test]
    public void RunWithLogsMultiple_WithSingleTransformation_ShouldBehaveLikeRunWithLogs()
    {
        // Arrange
        var input = WrapWithLogs(10);

        // Act
        var result = RunWithLogsMultiple(input, Square);

        // Assert
        Assert.That(result.Result, Is.EqualTo(100));
        Assert.That(result.Logs, Has.Length.EqualTo(1));
        Assert.That(result.Logs[0], Is.EqualTo("Squared 10 to get 100"));
    }

    [Test]
    public void RunWithLogsMultiple_WithNoTransformations_ShouldReturnOriginalInput()
    {
        // Arrange
        var input = WrapWithLogs(42);

        // Act
        var result = RunWithLogsMultiple(input);

        // Assert
        Assert.That(result.Result, Is.EqualTo(42));
        Assert.That(result.Logs, Is.Empty);
    }

    [Test]
    public void MultipleOperations_ShouldPreserveOrderOfLogs()
    {
        // Arrange
        var input = WrapWithLogs(2);

        // Act
        // 2 -> Square -> 4 -> AddOne -> 5 -> Square -> 25
        var result = RunWithLogsMultiple(input, Square, AddOne, Square);

        // Assert
        Assert.That(result.Result, Is.EqualTo(25));
        Assert.That(result.Logs, Has.Length.EqualTo(3));
        Assert.That(result.Logs[0], Is.EqualTo("Squared 2 to get 4"));
        Assert.That(result.Logs[1], Is.EqualTo("Added 1 to 4 to get 5"));
        Assert.That(result.Logs[2], Is.EqualTo("Squared 5 to get 25"));
    }

    [Test]
    public void NumberWithLogs_WithRecordSyntax_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var result = new NumberWithLogs(42, ["Log 1", "Log 2"]);

        // Assert
        Assert.That(result.Result, Is.EqualTo(42));
        Assert.That(result.Logs, Has.Length.EqualTo(2));
        Assert.That(result.Logs[0], Is.EqualTo("Log 1"));
        Assert.That(result.Logs[1], Is.EqualTo("Log 2"));
    }

    [Test]
    public void NumberWithLogs_WithModification_ShouldSupportRecordWith()
    {
        // Arrange
        var original = new NumberWithLogs(10, ["Log 1"]);

        // Act
        var modified = original with { Result = 20 };

        // Assert
        Assert.That(modified.Result, Is.EqualTo(20));
        Assert.That(modified.Logs, Is.EqualTo(original.Logs));
        Assert.That(original.Result, Is.EqualTo(10)); // Original unchanged
    }
}
