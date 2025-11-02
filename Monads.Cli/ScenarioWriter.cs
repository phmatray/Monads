using Monads;
using Spectre.Console;

namespace Monads.Cli;

public static class ScenarioWriter
{
    public static void RunScenario()
    {
        AnsiConsole.MarkupLine("[bold yellow]Writer Monad Examples[/]");
        AnsiConsole.WriteLine();

        // Example 1: Basic Writer usage
        AnsiConsole.MarkupLine("[bold]1. Basic Writer Usage[/]");
        var writer1 = Writer<int>.Return(5);
        var writer2 = Writer<int>.Create(10, "Created with value 10");

        DisplayWriter("Initial value", writer1);
        DisplayWriter("Value with log", writer2);
        AnsiConsole.WriteLine();

        // Example 2: Map operation
        AnsiConsole.MarkupLine("[bold]2. Map Operation[/]");
        var mapped = writer2.Map(x => x * 2);
        DisplayWriter("After mapping (*2)", mapped);
        AnsiConsole.WriteLine();

        // Example 3: Bind operation (accumulating logs)
        AnsiConsole.MarkupLine("[bold]3. Bind Operation (Log Accumulation)[/]");
        var result = Writer<int>.Return(5)
            .Bind(x => Writer<int>.Create(x + 1, $"Added 1 to {x}"))
            .Bind(x => Writer<int>.Create(x * 2, $"Multiplied {x} by 2"))
            .Bind(x => Writer<int>.Create(x - 3, $"Subtracted 3 from {x}"));

        DisplayWriter("Computation with logs", result);
        AnsiConsole.WriteLine();

        // Example 4: LINQ query syntax
        AnsiConsole.MarkupLine("[bold]4. LINQ Query Syntax[/]");
        var linqResult =
            from x in Writer<int>.Create(5, "Started with 5")
            from y in Writer<int>.Create(10, "Added 10")
            select x + y;

        DisplayWriter("LINQ computation", linqResult);
        AnsiConsole.WriteLine();

        // Example 5: Complex computation with logging
        AnsiConsole.MarkupLine("[bold]5. Complex Computation (Price Calculator)[/]");
        var finalPrice = CalculatePrice(100.0);
        DisplayWriterDouble("Price calculation", finalPrice);
        AnsiConsole.WriteLine();

        // Example 6: Generic Writer with custom log type
        AnsiConsole.MarkupLine("[bold]6. Generic Writer (Custom Log Type)[/]");
        var customWriter = new Writer<string, List<(DateTime, string)>>(
            "Result",
            new List<(DateTime, string)>
            {
                (DateTime.Now, "Operation 1"),
                (DateTime.Now.AddSeconds(1), "Operation 2")
            });

        AnsiConsole.MarkupLine($"Value: [cyan]{customWriter.Value}[/]");
        AnsiConsole.MarkupLine("Timestamped logs:");
        foreach (var (timestamp, message) in customWriter.Log)
        {
            AnsiConsole.MarkupLine($"  [{timestamp:HH:mm:ss}] [grey]{message}[/]");
        }
        AnsiConsole.WriteLine();

        // Example 7: Practical example - Mathematical expression evaluation
        AnsiConsole.MarkupLine("[bold]7. Mathematical Expression Evaluation[/]");
        var expr = EvaluateExpression();
        DisplayWriter("Expression: (5 + 3) * 2 - 4", expr);
    }

    static Writer<double> CalculatePrice(double basePrice)
    {
        return Writer<double>.Create(basePrice, $"Base price: ${basePrice}")
            .Bind(price => ApplyDiscount(price, 0.1))
            .Bind(price => ApplyTax(price, 0.08))
            .Bind(price => ApplyShipping(price, 5.0));
    }

    static Writer<double> ApplyDiscount(double price, double discountRate)
    {
        var discounted = price * (1 - discountRate);
        return Writer<double>.Create(
            discounted,
            $"Applied {discountRate * 100}% discount: ${price:F2} → ${discounted:F2}"
        );
    }

    static Writer<double> ApplyTax(double price, double taxRate)
    {
        var withTax = price * (1 + taxRate);
        return Writer<double>.Create(
            withTax,
            $"Applied {taxRate * 100}% tax: ${price:F2} → ${withTax:F2}"
        );
    }

    static Writer<double> ApplyShipping(double price, double shippingCost)
    {
        var total = price + shippingCost;
        return Writer<double>.Create(
            total,
            $"Added shipping ${shippingCost:F2}: ${price:F2} → ${total:F2}"
        );
    }

    static Writer<int> EvaluateExpression()
    {
        return Writer<int>.Create(5, "Value: 5")
            .Bind(x => Writer<int>.Create(x + 3, $"{x} + 3 = {x + 3}"))
            .Bind(x => Writer<int>.Create(x * 2, $"{x} * 2 = {x * 2}"))
            .Bind(x => Writer<int>.Create(x - 4, $"{x} - 4 = {x - 4}"));
    }

    static void DisplayWriter(string title, Writer<int> writer)
    {
        AnsiConsole.MarkupLine($"[bold]{title}:[/]");
        AnsiConsole.MarkupLine($"  Value: [green]{writer.Value}[/]");
        if (writer.Log.Any())
        {
            AnsiConsole.MarkupLine("  Logs:");
            foreach (var log in writer.Log)
            {
                AnsiConsole.MarkupLine($"    [grey]• {log}[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("  [grey](no logs)[/]");
        }
    }

    static void DisplayWriterDouble(string title, Writer<double> writer)
    {
        AnsiConsole.MarkupLine($"[bold]{title}:[/]");
        AnsiConsole.MarkupLine($"  Final Price: [green]${writer.Value:F2}[/]");
        if (writer.Log.Any())
        {
            AnsiConsole.MarkupLine("  Calculation steps:");
            foreach (var log in writer.Log)
            {
                AnsiConsole.MarkupLine($"    [grey]• {log}[/]");
            }
        }
    }
}
