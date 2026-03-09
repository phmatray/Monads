using Monads;

namespace Monads.Tests;

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

    [Fact]
    public void WrapWithLogs_ShouldCreateNumberWithLogsWithEmptyLogs()
    {
        // Arrange
        var value = 5;

        // Act
        var result = WrapWithLogs(value);

        // Assert
        Assert.Equal(5, result.Result);
        Assert.Empty(result.Logs);
    }

    [Fact]
    public void AddOne_ShouldIncrementValueAndLogOperation()
    {
        // Arrange
        var value = 5;

        // Act
        var result = AddOne(value);

        // Assert
        Assert.Equal(6, result.Result);
        Assert.Equal(1, result.Logs.Length);
        Assert.Equal("Added 1 to 5 to get 6", result.Logs[0]);
    }

    [Fact]
    public void Square_ShouldSquareValueAndLogOperation()
    {
        // Arrange
        var value = 4;

        // Act
        var result = Square(value);

        // Assert
        Assert.Equal(16, result.Result);
        Assert.Equal(1, result.Logs.Length);
        Assert.Equal("Squared 4 to get 16", result.Logs[0]);
    }

    [Fact]
    public void MultiplyByThree_ShouldMultiplyValueAndLogOperation()
    {
        // Arrange
        var value = 7;

        // Act
        var result = MultiplyByThree(value);

        // Assert
        Assert.Equal(21, result.Result);
        Assert.Equal(1, result.Logs.Length);
        Assert.Equal("Multiplied 7 by 3 to get 21", result.Logs[0]);
    }

    [Fact]
    public void RunWithLogs_ShouldCombineLogsFromInputAndTransformation()
    {
        // Arrange
        var input = new NumberWithLogs(5, ["Initial log"]);

        // Act
        var result = RunWithLogs(input, AddOne);

        // Assert
        Assert.Equal(6, result.Result);
        Assert.Equal(2, result.Logs.Length);
        Assert.Equal("Initial log", result.Logs[0]);
        Assert.Equal("Added 1 to 5 to get 6", result.Logs[1]);
    }

    [Fact]
    public void RunWithLogsMultiple_ShouldApplyAllTransformationsInOrder()
    {
        // Arrange
        var input = WrapWithLogs(5);

        // Act
        // 5 -> AddOne -> 6 -> Square -> 36 -> MultiplyByThree -> 108
        var result = RunWithLogsMultiple(input, AddOne, Square, MultiplyByThree);

        // Assert
        Assert.Equal(108, result.Result);
        Assert.Equal(3, result.Logs.Length);
        Assert.Equal("Added 1 to 5 to get 6", result.Logs[0]);
        Assert.Equal("Squared 6 to get 36", result.Logs[1]);
        Assert.Equal("Multiplied 36 by 3 to get 108", result.Logs[2]);
    }

    [Fact]
    public void RunWithLogsMultiple_WithSingleTransformation_ShouldBehaveLikeRunWithLogs()
    {
        // Arrange
        var input = WrapWithLogs(10);

        // Act
        var result = RunWithLogsMultiple(input, Square);

        // Assert
        Assert.Equal(100, result.Result);
        Assert.Equal(1, result.Logs.Length);
        Assert.Equal("Squared 10 to get 100", result.Logs[0]);
    }

    [Fact]
    public void RunWithLogsMultiple_WithNoTransformations_ShouldReturnOriginalInput()
    {
        // Arrange
        var input = WrapWithLogs(42);

        // Act
        var result = RunWithLogsMultiple(input);

        // Assert
        Assert.Equal(42, result.Result);
        Assert.Empty(result.Logs);
    }

    [Fact]
    public void MultipleOperations_ShouldPreserveOrderOfLogs()
    {
        // Arrange
        var input = WrapWithLogs(2);

        // Act
        // 2 -> Square -> 4 -> AddOne -> 5 -> Square -> 25
        var result = RunWithLogsMultiple(input, Square, AddOne, Square);

        // Assert
        Assert.Equal(25, result.Result);
        Assert.Equal(3, result.Logs.Length);
        Assert.Equal("Squared 2 to get 4", result.Logs[0]);
        Assert.Equal("Added 1 to 4 to get 5", result.Logs[1]);
        Assert.Equal("Squared 5 to get 25", result.Logs[2]);
    }

    [Fact]
    public void NumberWithLogs_WithRecordSyntax_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var result = new NumberWithLogs(42, ["Log 1", "Log 2"]);

        // Assert
        Assert.Equal(42, result.Result);
        Assert.Equal(2, result.Logs.Length);
        Assert.Equal("Log 1", result.Logs[0]);
        Assert.Equal("Log 2", result.Logs[1]);
    }

    [Fact]
    public void NumberWithLogs_WithModification_ShouldSupportRecordWith()
    {
        // Arrange
        var original = new NumberWithLogs(10, ["Log 1"]);

        // Act
        var modified = original with { Result = 20 };

        // Assert
        Assert.Equal(20, modified.Result);
        Assert.Equal(original.Logs, modified.Logs);
        Assert.Equal(10, original.Result); // Original unchanged
    }
}
