Console.WriteLine("Monads in C#");

Func<int, NumberWithLogs> square = x =>
{
    return new NumberWithLogs(
        x * x,
        [$"Squared {x} to get {x * x}"]);
};

Func<int, NumberWithLogs> addOne = x =>
{
    return new NumberWithLogs(
        x + 1,
        [$"Added 1 to {x} to get {x + 1}"]);
};

Func<int, NumberWithLogs> multiplyByThree = x =>
{
    return new NumberWithLogs(
        x * 3,
        [$"Multiplied {x} by 3 to get {x * 3}"]);
};

Func<int, NumberWithLogs> wrapWithLogs = x =>
{
    return new NumberWithLogs(x, []);
};

Func<NumberWithLogs, Func<int, NumberWithLogs>, NumberWithLogs> runWithLogs = (input, transform) =>
{
    var newNumberWithLogs = transform(input.Result);
    return new NumberWithLogs(
        newNumberWithLogs.Result,
        input.Logs.Concat(newNumberWithLogs.Logs).ToArray());
};

// old style
// var result1 = square(square(wrapWithLogs(2)));
// new style
// var result1 = runWithLogs(wrapWithLogs(2), square, square);
var result1A = wrapWithLogs(5);
var result1B = runWithLogs(result1A, addOne);
var result1C = runWithLogs(result1B, square);
var result1D = runWithLogs(result1C, multiplyByThree);
Console.WriteLine();
Console.WriteLine($"Logs 1: {string.Join(", ", result1D.Logs)}");
Console.WriteLine($"Result 1: {result1D.Result}");

// old style
// var result2 = addOne(wrapWithLogs(5));
// new style
var result2 = runWithLogs(wrapWithLogs(5), addOne);
Console.WriteLine();
Console.WriteLine($"Logs 2: {string.Join(", ", result2.Logs)}");
Console.WriteLine($"Result 2: {result2.Result}");

internal record NumberWithLogs(int Result, string[] Logs);