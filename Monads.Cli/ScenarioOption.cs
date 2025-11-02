using Monads;
using Spectre.Console;

namespace Monads.Cli;

public static class ScenarioOption
{
    public static void RunScenario()
    {
        AnsiConsole.MarkupLine("[bold yellow]Option Monad Examples[/]");
        AnsiConsole.WriteLine();

        // Example 1: Basic Option usage
        AnsiConsole.MarkupLine("[bold]1. Basic Option Usage[/]");
        var someValue = Option<int>.Some(42);
        var noneValue = Option<int>.None();

        AnsiConsole.MarkupLine($"Some value: [green]{someValue.GetValueOrDefault(-1)}[/]");
        AnsiConsole.MarkupLine($"None value: [red]{noneValue.GetValueOrDefault(-1)}[/]");
        AnsiConsole.WriteLine();

        // Example 2: Map operation
        AnsiConsole.MarkupLine("[bold]2. Map Operation[/]");
        var doubled = someValue.Map(x => x * 2);
        AnsiConsole.MarkupLine($"Original: [cyan]42[/] → Doubled: [cyan]{doubled.GetValueOrDefault(0)}[/]");
        AnsiConsole.WriteLine();

        // Example 3: Bind (flatMap) operation
        AnsiConsole.MarkupLine("[bold]3. Bind Operation (Chaining)[/]");
        var result = someValue
            .Bind(x => Option<int>.Some(x + 10))
            .Bind(x => Option<int>.Some(x * 2));
        AnsiConsole.MarkupLine($"42 → add 10 → multiply by 2 = [green]{result.GetValueOrDefault(0)}[/]");
        AnsiConsole.WriteLine();

        // Example 4: LINQ query syntax
        AnsiConsole.MarkupLine("[bold]4. LINQ Query Syntax[/]");
        var linqResult =
            from x in Option<int>.Some(5)
            from y in Option<int>.Some(10)
            select x + y;
        AnsiConsole.MarkupLine($"5 + 10 = [green]{linqResult.GetValueOrDefault(0)}[/]");
        AnsiConsole.WriteLine();

        // Example 5: Handling null values
        AnsiConsole.MarkupLine("[bold]5. Converting Nullable Values[/]");
        string? nullString = null;
        string? validString = "Hello, World!";

        var optionFromNull = nullString.ToOption();
        var optionFromValid = validString.ToOption();

        optionFromNull.Match(
            onSome: str => AnsiConsole.MarkupLine($"Value: [green]{str}[/]"),
            onNone: () => AnsiConsole.MarkupLine("[red]No value (from null)[/]")
        );

        optionFromValid.Match(
            onSome: str => AnsiConsole.MarkupLine($"Value: [green]{str}[/]"),
            onNone: () => AnsiConsole.MarkupLine("[red]No value[/]")
        );
        AnsiConsole.WriteLine();

        // Example 6: Safe division
        AnsiConsole.MarkupLine("[bold]6. Safe Division (Real-World Example)[/]");
        DisplayDivisionResult(10, 2);
        DisplayDivisionResult(10, 0);
        AnsiConsole.WriteLine();

        // Example 7: Filtering
        AnsiConsole.MarkupLine("[bold]7. Filtering with Where[/]");
        var filtered1 = Option<int>.Some(15).Where(x => x > 10);
        var filtered2 = Option<int>.Some(5).Where(x => x > 10);

        AnsiConsole.MarkupLine($"15 > 10? [green]{filtered1.IsSome}[/]");
        AnsiConsole.MarkupLine($"5 > 10? [red]{filtered2.IsSome}[/]");
        AnsiConsole.WriteLine();

        // Example 8: Working with collections
        AnsiConsole.MarkupLine("[bold]8. Working with Collections[/]");
        var numbers = new[] { "1", "2", "not a number", "4", "5" };
        var parsedNumbers = numbers
            .Select(TryParseInt)
            .Choose(opt => opt)
            .ToList();

        AnsiConsole.MarkupLine($"Parsed numbers: [cyan]{string.Join(", ", parsedNumbers)}[/]");
    }

    static Option<int> SafeDivide(int numerator, int denominator)
        => denominator == 0
            ? Option<int>.None()
            : Option<int>.Some(numerator / denominator);

    static void DisplayDivisionResult(int numerator, int denominator)
    {
        var result = SafeDivide(numerator, denominator);
        result.Match(
            onSome: value => AnsiConsole.MarkupLine($"{numerator} / {denominator} = [green]{value}[/]"),
            onNone: () => AnsiConsole.MarkupLine($"{numerator} / {denominator} = [red]Cannot divide by zero![/]")
        );
    }

    static Option<int> TryParseInt(string str)
        => int.TryParse(str, out var result)
            ? Option<int>.Some(result)
            : Option<int>.None();
}
