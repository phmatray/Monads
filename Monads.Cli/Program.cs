using Monads;
using Monads.Cli;
using Spectre.Console;

AnsiConsole.Write(
    new FigletText("Monads in C#")
        .Centered()
        .Color(Color.Blue));

AnsiConsole.MarkupLine("[grey]A comprehensive functional programming library[/]");
AnsiConsole.WriteLine();

var scenario = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Which [green]monad scenario[/] would you like to run?")
        .PageSize(10)
        .AddChoices(new[]
        {
            "Option Monad - Safe null handling",
            "Result Monad - Error handling without exceptions",
            "Writer Monad - Computation with logging",
            "Validation Monad - Input validation",
            "Legacy: Logging Monad (Basic)",
            "Run All Scenarios"
        }));

AnsiConsole.WriteLine();

switch (scenario)
{
    case "Option Monad - Safe null handling":
        AnsiConsole.Write(new Rule("[blue]Option Monad[/]").LeftJustified());
        AnsiConsole.WriteLine();
        ScenarioOption.RunScenario();
        break;

    case "Result Monad - Error handling without exceptions":
        AnsiConsole.Write(new Rule("[blue]Result Monad[/]").LeftJustified());
        AnsiConsole.WriteLine();
        ScenarioResult.RunScenario();
        break;

    case "Writer Monad - Computation with logging":
        AnsiConsole.Write(new Rule("[blue]Writer Monad[/]").LeftJustified());
        AnsiConsole.WriteLine();
        ScenarioWriter.RunScenario();
        break;

    case "Validation Monad - Input validation":
        AnsiConsole.Write(new Rule("[blue]Validation Monad[/]").LeftJustified());
        AnsiConsole.WriteLine();
        ScenarioValidation.RunScenario();
        break;

    case "Legacy: Logging Monad (Basic)":
        AnsiConsole.Write(new Rule("[blue]Legacy Logging Monad[/]").LeftJustified());
        AnsiConsole.WriteLine();
        ScenarioBasic.RunScenario();
        break;

    case "Run All Scenarios":
        RunAllScenarios();
        break;
}

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[green]✓ Done![/]");
AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");

void RunAllScenarios()
{
    AnsiConsole.Write(new Rule("[blue]Option Monad[/]").LeftJustified());
    AnsiConsole.WriteLine();
    ScenarioOption.RunScenario();

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[blue]Result Monad[/]").LeftJustified());
    AnsiConsole.WriteLine();
    ScenarioResult.RunScenario();

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[blue]Writer Monad[/]").LeftJustified());
    AnsiConsole.WriteLine();
    ScenarioWriter.RunScenario();

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[blue]Validation Monad[/]").LeftJustified());
    AnsiConsole.WriteLine();
    ScenarioValidation.RunScenario();

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[blue]Legacy Logging Monad[/]").LeftJustified());
    AnsiConsole.WriteLine();
    ScenarioBasic.RunScenario();
}
