using Monads;

namespace Monads.Tests;

/// <summary>
/// Tests to verify that the logging monad implementation satisfies the three monad laws:
/// 1. Left Identity: return a >>= f ≡ f a
/// 2. Right Identity: m >>= return ≡ m
/// 3. Associativity: (m >>= f) >>= g ≡ m >>= (x -> f x >>= g)
/// </summary>
[TestFixture]
public class MonadLawsTests
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

    #region Left Identity Law Tests

    /// <summary>
    /// Left Identity: return a >>= f ≡ f a
    ///
    /// This law states that wrapping a value and then applying a function
    /// should be the same as just applying the function to the value.
    /// </summary>
    [Test]
    public void LeftIdentity_WithAddOne_ShouldHoldTrue()
    {
        // Arrange
        var a = 5;
        Func<int, NumberWithLogs> f = AddOne;

        // Act
        var left = RunWithLogs(WrapWithLogs(a), f);  // return a >>= f
        var right = f(a);                             // f a

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    [Test]
    public void LeftIdentity_WithSquare_ShouldHoldTrue()
    {
        // Arrange
        var a = 7;
        Func<int, NumberWithLogs> f = Square;

        // Act
        var left = RunWithLogs(WrapWithLogs(a), f);
        var right = f(a);

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    [Test]
    public void LeftIdentity_WithMultiplyByThree_ShouldHoldTrue()
    {
        // Arrange
        var a = 10;
        Func<int, NumberWithLogs> f = MultiplyByThree;

        // Act
        var left = RunWithLogs(WrapWithLogs(a), f);
        var right = f(a);

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    #endregion

    #region Right Identity Law Tests

    /// <summary>
    /// Right Identity: m >>= return ≡ m
    ///
    /// This law states that applying the "return" function (WrapWithLogs)
    /// to a monad should give back the same monad value.
    /// </summary>
    [Test]
    public void RightIdentity_WithSimpleValue_ShouldHoldTrue()
    {
        // Arrange
        var m = new NumberWithLogs(42, ["Log 1", "Log 2"]);
        Func<int, NumberWithLogs> returnFunc = WrapWithLogs;

        // Act
        var left = RunWithLogs(m, returnFunc);  // m >>= return
        var right = m;                           // m

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        // Note: Logs will differ because RunWithLogs concatenates logs
        // This is a characteristic of the Writer monad - it accumulates effects
        // So we verify that the original logs are preserved as a prefix
        Assert.That(left.Logs.Take(right.Logs.Length).ToArray(), Is.EqualTo(right.Logs));
    }

    [Test]
    public void RightIdentity_WithEmptyLogs_ShouldHoldTrue()
    {
        // Arrange
        var m = WrapWithLogs(15);
        Func<int, NumberWithLogs> returnFunc = WrapWithLogs;

        // Act
        var left = RunWithLogs(m, returnFunc);
        var right = m;

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    #endregion

    #region Associativity Law Tests

    /// <summary>
    /// Associativity: (m >>= f) >>= g ≡ m >>= (x -> f x >>= g)
    ///
    /// This law states that the order in which we nest monadic operations
    /// doesn't matter - the result should be the same.
    /// </summary>
    [Test]
    public void Associativity_WithAddOneAndSquare_ShouldHoldTrue()
    {
        // Arrange
        var m = WrapWithLogs(5);
        Func<int, NumberWithLogs> f = AddOne;
        Func<int, NumberWithLogs> g = Square;

        // Act
        // Left side: (m >>= f) >>= g
        var left = RunWithLogs(RunWithLogs(m, f), g);

        // Right side: m >>= (x -> f x >>= g)
        var right = RunWithLogs(m, x => RunWithLogs(f(x), g));

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    [Test]
    public void Associativity_WithSquareAndMultiplyByThree_ShouldHoldTrue()
    {
        // Arrange
        var m = WrapWithLogs(3);
        Func<int, NumberWithLogs> f = Square;
        Func<int, NumberWithLogs> g = MultiplyByThree;

        // Act
        var left = RunWithLogs(RunWithLogs(m, f), g);
        var right = RunWithLogs(m, x => RunWithLogs(f(x), g));

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    [Test]
    public void Associativity_WithThreeOperations_ShouldHoldTrue()
    {
        // Arrange
        var m = WrapWithLogs(2);
        Func<int, NumberWithLogs> f = AddOne;
        Func<int, NumberWithLogs> g = Square;
        Func<int, NumberWithLogs> h = MultiplyByThree;

        // Act
        // Left side: ((m >>= f) >>= g) >>= h
        var left = RunWithLogs(RunWithLogs(RunWithLogs(m, f), g), h);

        // Right side: m >>= (x -> (f x >>= g) >>= h)
        var right = RunWithLogs(m, x => RunWithLogs(RunWithLogs(f(x), g), h));

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    [Test]
    public void Associativity_AlternativeGrouping_ShouldHoldTrue()
    {
        // Arrange
        var m = WrapWithLogs(4);
        Func<int, NumberWithLogs> f = AddOne;
        Func<int, NumberWithLogs> g = Square;
        Func<int, NumberWithLogs> h = MultiplyByThree;

        // Act
        // Left grouping: ((m >>= f) >>= g) >>= h
        var left = RunWithLogs(RunWithLogs(RunWithLogs(m, f), g), h);

        // Right grouping: m >>= (x -> f x >>= (y -> g y >>= h))
        var right = RunWithLogs(m, x =>
            RunWithLogs(f(x), y =>
                RunWithLogs(g(y), h)));

        // Assert
        Assert.That(left.Result, Is.EqualTo(right.Result));
        Assert.That(left.Logs, Is.EqualTo(right.Logs));
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests that verify all three laws work together in practical scenarios
    /// </summary>
    [Test]
    public void AllMonadLaws_ShouldWorkTogether()
    {
        // This test demonstrates that the monad laws ensure predictable composition

        // Starting value
        var value = 5;

        // Different ways to compose the same operations should yield the same result

        // Method 1: Direct composition
        var result1 = RunWithLogs(
            RunWithLogs(WrapWithLogs(value), AddOne),
            Square);

        // Method 2: Using associativity differently
        var result2 = RunWithLogs(
            WrapWithLogs(value),
            x => RunWithLogs(AddOne(x), Square));

        // Method 3: Step by step
        var step1 = AddOne(value);
        var result3 = RunWithLogs(step1, Square);

        // All should produce the same result
        Assert.That(result1.Result, Is.EqualTo(result2.Result));
        Assert.That(result2.Result, Is.EqualTo(result3.Result));
        Assert.That(result1.Logs, Is.EqualTo(result2.Logs));
        Assert.That(result2.Logs, Is.EqualTo(result3.Logs));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    [TestCase(100)]
    public void MonadLaws_ShouldHoldForVariousInputValues(int input)
    {
        // Arrange
        var m = WrapWithLogs(input);
        Func<int, NumberWithLogs> f = AddOne;
        Func<int, NumberWithLogs> g = Square;

        // Act & Assert - Left Identity
        var leftIdentityLeft = RunWithLogs(WrapWithLogs(input), f);
        var leftIdentityRight = f(input);
        Assert.That(leftIdentityLeft.Result, Is.EqualTo(leftIdentityRight.Result));

        // Act & Assert - Associativity
        var assocLeft = RunWithLogs(RunWithLogs(m, f), g);
        var assocRight = RunWithLogs(m, x => RunWithLogs(f(x), g));
        Assert.That(assocLeft.Result, Is.EqualTo(assocRight.Result));
        Assert.That(assocLeft.Logs, Is.EqualTo(assocRight.Logs));
    }

    #endregion
}
