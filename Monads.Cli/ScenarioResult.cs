using Monads;
using Spectre.Console;

namespace Monads.Cli;

public static class ScenarioResult
{
    public static void RunScenario()
    {
        AnsiConsole.MarkupLine("[bold yellow]Result Monad Examples[/]");
        AnsiConsole.WriteLine();

        // Example 1: Basic Result usage
        AnsiConsole.MarkupLine("[bold]1. Basic Result Usage[/]");
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure("Something went wrong");

        AnsiConsole.MarkupLine($"Success value: [green]{success.GetValueOrDefault(-1)}[/]");
        AnsiConsole.MarkupLine($"Failure value: [red]{failure.GetValueOrDefault(-1)}[/]");
        AnsiConsole.WriteLine();

        // Example 2: Map operation
        AnsiConsole.MarkupLine("[bold]2. Map Operation[/]");
        var mapped = success.Map(x => x * 2);
        mapped.Match(
            onSuccess: value => AnsiConsole.MarkupLine($"Mapped value: [green]{value}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Error: [red]{error}[/]")
        );
        AnsiConsole.WriteLine();

        // Example 3: Bind (Railway-Oriented Programming)
        AnsiConsole.MarkupLine("[bold]3. Railway-Oriented Programming (Bind)[/]");
        var pipeline = ValidateAge(25)
            .Bind(age => ValidateEmail("user@example.com"))
            .Bind(email => CreateUser(email));

        pipeline.Match(
            onSuccess: user => AnsiConsole.MarkupLine($"Created user: [green]{user}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Validation failed: [red]{error}[/]")
        );
        AnsiConsole.WriteLine();

        // Example 4: Failed validation chain
        AnsiConsole.MarkupLine("[bold]4. Failed Validation Chain[/]");
        var failedPipeline = ValidateAge(15) // This will fail
            .Bind(age => ValidateEmail("user@example.com"))
            .Bind(email => CreateUser(email));

        failedPipeline.Match(
            onSuccess: user => AnsiConsole.MarkupLine($"Created user: [green]{user}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Validation failed: [red]{error}[/]")
        );
        AnsiConsole.WriteLine();

        // Example 5: LINQ query syntax
        AnsiConsole.MarkupLine("[bold]5. LINQ Query Syntax[/]");
        var linqResult =
            from x in Divide(10, 2)
            from y in Divide(20, 4)
            select x + y;

        linqResult.Match(
            onSuccess: value => AnsiConsole.MarkupLine($"10/2 + 20/4 = [green]{value}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Error: [red]{error}[/]")
        );
        AnsiConsole.WriteLine();

        // Example 6: Error recovery with OrElse
        AnsiConsole.MarkupLine("[bold]6. Error Recovery with OrElse[/]");
        var recovered = Divide(10, 0)
            .OrElse(error => Result<int>.Success(0)); // Recover from error

        recovered.Match(
            onSuccess: value => AnsiConsole.MarkupLine($"Recovered value: [green]{value}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Error: [red]{error}[/]")
        );
        AnsiConsole.WriteLine();

        // Example 7: Try-Catch wrapper
        AnsiConsole.MarkupLine("[bold]7. Try-Catch Wrapper[/]");
        var tryResult1 = Result<int>.Try(() => int.Parse("123"));
        var tryResult2 = Result<int>.Try(() => int.Parse("not a number"));

        tryResult1.Match(
            onSuccess: value => AnsiConsole.MarkupLine($"Parsed: [green]{value}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Parse error: [red]{error}[/]")
        );

        tryResult2.Match(
            onSuccess: value => AnsiConsole.MarkupLine($"Parsed: [green]{value}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Parse error: [red]{error}[/]")
        );
        AnsiConsole.WriteLine();

        // Example 8: Converting between Option and Result
        AnsiConsole.MarkupLine("[bold]8. Converting between Option and Result[/]");
        var option = Option<int>.Some(42);
        var resultFromOption = option.ToResult("Value was None");

        resultFromOption.Match(
            onSuccess: value => AnsiConsole.MarkupLine($"From Option: [green]{value}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"Error: [red]{error}[/]")
        );

        var resultToOption = success.ToOption();
        resultToOption.Match(
            onSome: value => AnsiConsole.MarkupLine($"To Option: [green]{value}[/]"),
            onNone: () => AnsiConsole.MarkupLine("[red]None[/]")
        );
        AnsiConsole.WriteLine();

        // Example 9: Working with collections
        AnsiConsole.MarkupLine("[bold]9. Combining Results from Collections[/]");
        var numbers = new[] { 10, 20, 30 };
        var results = numbers.Select(n => Divide(n, 2)).Sequence();

        results.Match(
            onSuccess: values => AnsiConsole.MarkupLine($"All divisions succeeded: [green]{string.Join(", ", values)}[/]"),
            onFailure: error => AnsiConsole.MarkupLine($"At least one division failed: [red]{error}[/]")
        );
    }

    static Result<int> Divide(int numerator, int denominator)
        => denominator == 0
            ? Result<int>.Failure("Cannot divide by zero")
            : Result<int>.Success(numerator / denominator);

    static Result<int> ValidateAge(int age)
        => age >= 18
            ? Result<int>.Success(age)
            : Result<int>.Failure($"Age {age} is below minimum (18)");

    static Result<string> ValidateEmail(string email)
        => email.Contains('@')
            ? Result<string>.Success(email)
            : Result<string>.Failure($"Invalid email: {email}");

    static Result<string> CreateUser(string email)
        => Result<string>.Success($"User created with email: {email}");
}
