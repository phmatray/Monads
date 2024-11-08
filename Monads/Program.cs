Console.WriteLine("Monads in C#");
Console.WriteLine();

// Running a series of transformations on the initial value of 5
// Here we use multiple functions: AddOne, Square, and MultiplyByThree
var result1 = RunWithLogsMultiple(WrapWithLogs(5), AddOne, Square, MultiplyByThree);
PrintResult(result1);

Console.WriteLine();

// Running a single transformation on the initial value of 5
// Here we use only the AddOne function
var result2 = RunWithLogs(WrapWithLogs(5), AddOne);
PrintResult(result2);

return;

// Function to print the final result and the logs collected during transformations
void PrintResult(NumberWithLogs result)
{
    Console.WriteLine($"Logs: {string.Join(", ", result.Logs)}");
    Console.WriteLine($"Result: {result.Result}");
}

// Function to square a number and return the result along with a log message
NumberWithLogs Square(int x)
    => new(x * x, [$"Squared {x} to get {x * x}"]);

// Function to add one to a number and return the result along with a log message
NumberWithLogs AddOne(int x)
    => new(x + 1, [$"Added 1 to {x} to get {x + 1}"]);

// Function to multiply a number by three and return the result along with a log message
NumberWithLogs MultiplyByThree(int x)
    => new(x * 3, [$"Multiplied {x} by 3 to get {x * 3}"]);

// Function to wrap an integer value with an empty log, used to initialize the monad
NumberWithLogs WrapWithLogs(int x)
    => new(x, []);

// Function to apply a single transformation to a NumberWithLogs instance
// Combines the input logs with the logs from the transformation
NumberWithLogs RunWithLogs(NumberWithLogs input, Func<int, NumberWithLogs> transform)
{
    // Apply the transformation to get the new result
    var transformed = transform(input.Result);
    // Combine the logs from the input and the transformation
    return transformed with
    {
        Logs = input.Logs.Concat(transformed.Logs).ToArray()
    };
}

// Function to apply multiple transformations to a NumberWithLogs instance
// Uses Aggregate to sequentially apply each function and collect all logs
NumberWithLogs RunWithLogsMultiple(NumberWithLogs input, params Func<int, NumberWithLogs>[] transforms)
    => transforms.Aggregate(input, RunWithLogs);

// Record type to hold the result and associated logs
internal record NumberWithLogs(int Result, string[] Logs);
