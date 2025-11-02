# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a comprehensive **functional programming library** for C# that implements monad patterns. The library provides type-safe, composable abstractions for handling common programming challenges like null values, error handling, logging, and validation.

## Core Monads

The library implements four primary monads:

1. **Option<T>** - Safe null handling (Maybe pattern)
2. **Result<T, TError>** - Error handling without exceptions (Either pattern)
3. **Writer<T, TLog>** - Computation with logging/tracing
4. **ValidationResult** - Input validation with error accumulation

## Architecture

The solution consists of three projects:

### Core Library - Monads/

Class library containing all monad types and extensions:

**Monad Types:**
- `Option.cs` - Option/Maybe monad for handling optional values
- `Result.cs` - Result/Either monad for error handling
- `Writer.cs` - Writer monad for logging computations
- `ValidationResult.cs` - Validation monad (legacy, for backwards compatibility)
- `NumberWithLogs.cs` - Simple writer monad (legacy, for backwards compatibility)

**Extensions:**
- `AsyncExtensions.cs` - Async/await support for all monads
- `FunctionalExtensions.cs` - Functional programming utilities (composition, currying, etc.)
- `OptionExtensions.cs` - Extension methods for Option monad (in Option.cs)
- `ResultExtensions.cs` - Extension methods for Result monad (in Result.cs)

### CLI Application - Monads.Cli/

Console application with Spectre.Console demonstrating all monad types:
- `Program.cs` - Interactive menu to run scenarios
- `ScenarioOption.cs` - Option monad demonstrations
- `ScenarioResult.cs` - Result monad demonstrations
- `ScenarioWriter.cs` - Writer monad demonstrations
- `ScenarioValidation.cs` - Validation monad demonstrations
- `ScenarioBasic.cs` - Legacy logging monad (NumberWithLogs)

### Test Project - Monads.Tests/

NUnit test project with comprehensive test coverage:
- `LoggingMonadTests.cs` - Tests for the legacy logging monad
- `ValidationMonadTests.cs` - Tests for the validation monad
- `MonadLawsTests.cs` - Verifies the three monad laws

## Key Features

### LINQ Query Syntax Support

All monads support LINQ query syntax through `Select` and `SelectMany` implementations:

```csharp
// Option monad
var result =
    from x in Option<int>.Some(5)
    from y in Option<int>.Some(10)
    select x + y;

// Result monad
var result =
    from x in Divide(10, 2)
    from y in Divide(20, 4)
    select x + y;

// Writer monad
var result =
    from x in Writer<int>.Create(5, "Started with 5")
    from y in Writer<int>.Create(10, "Added 10")
    select x + y;
```

### Async/Await Support

All monads have async extensions for seamless async/await integration:

```csharp
// Async map
var result = await option.MapAsync(async x => await GetValueAsync(x));

// Async bind
var result = await result.BindAsync(async x => await ProcessAsync(x));

// Try-catch wrapper
var result = await AsyncExtensions.TryAsync(async () => await RiskyOperationAsync());
```

### Functional Programming Utilities

The library includes common functional programming patterns:

- **Function Composition**: `Compose`, `Pipe`
- **Currying**: `Curry`, `Uncurry`, `Partial`
- **Memoization**: `Memoize`
- **Side Effects**: `Tap`, `TapAsync`
- **Pattern Matching**: `Match` extensions for bool, nullable
- **Kleisli Composition**: `ComposeKleisli` for monadic functions
- **Collection Operations**: `Traverse`, `Sequence`, `Choose`

### Railway-Oriented Programming

The Result monad enables railway-oriented programming for robust error handling:

```csharp
var result = ValidateAge(25)
    .Bind(age => ValidateEmail("user@example.com"))
    .Bind(email => CreateUser(email))
    .Match(
        onSuccess: user => $"Created: {user}",
        onFailure: error => $"Failed: {error}"
    );
```

## Common Commands

### Build
```bash
dotnet build
```

### Run the Interactive Demo
```bash
dotnet run --project Monads.Cli/Monads.Cli.csproj
```

The CLI app provides an interactive menu to explore:
- Option Monad - Safe null handling
- Result Monad - Error handling without exceptions
- Writer Monad - Computation with logging
- Validation Monad - Input validation
- Legacy: Logging Monad (Basic)
- Run All Scenarios

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~LoggingMonadTests"
dotnet test --filter "FullyQualifiedName~ValidationMonadTests"
dotnet test --filter "FullyQualifiedName~MonadLawsTests"
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~MonadLawsTests.LeftIdentity_WithAddOne_ShouldHoldTrue"
```

### Clean Build Artifacts
```bash
dotnet clean
```

## Monad API Reference

### Option<T>

**Creation:**
- `Option<T>.Some(value)` - Create an option with a value
- `Option<T>.None()` - Create an empty option
- `value.ToOption()` - Extension method for nullable values

**Operations:**
- `Map<TResult>(Func<T, TResult>)` - Transform the value
- `Bind<TResult>(Func<T, Option<TResult>>)` - Chain operations
- `Where(Func<T, bool>)` - Filter based on predicate
- `Match<TResult>(Func<T, TResult>, Func<TResult>)` - Pattern matching
- `GetValueOrDefault(T)` - Extract value or use default
- `ToOption()`, `ToNullable()` - Conversions

### Result<T, TError>

**Creation:**
- `Result<T, TError>.Success(value)` - Create success
- `Result<T, TError>.Failure(error)` - Create failure
- `Result<T>.Try(Func<T>)` - Wrap try-catch
- `AsyncExtensions.TryAsync(Func<Task<T>>)` - Async try-catch

**Operations:**
- `Map<TResult>(Func<T, TResult>)` - Transform success value
- `MapError<TErrorResult>(Func<TError, TErrorResult>)` - Transform error
- `Bind<TResult>(Func<T, Result<TResult, TError>>)` - Chain operations
- `Match<TResult>(Func<T, TResult>, Func<TError, TResult>)` - Pattern matching
- `OrElse(Func<TError, Result<T, TError>>)` - Error recovery
- `ToOption()` - Convert to Option
- `Do(Action<T>)`, `DoOnError(Action<TError>)` - Side effects

### Writer<T, TLog>

**Creation:**
- `Writer<T>.Return(value)` - Create with empty log
- `Writer<T>.Create(value, log)` - Create with log entry
- `new Writer<T, TLog>(value, log)` - Generic version

**Operations:**
- `Map<TResult>(Func<T, TResult>)` - Transform value, preserve log
- `Bind<TResult>(Func<T, Writer<TResult>>)` - Chain with log accumulation
- `SelectMany` - LINQ support with automatic log concatenation

## Development Notes

- Target framework: .NET 9.0
- ImplicitUsings and Nullable reference types are enabled
- The CLI project uses **Spectre.Console** (version 0.53.0) for rich terminal UI
- All monads are immutable using C# record types
- All monads implement proper LINQ support (Select, SelectMany, Where)
- Comprehensive async/await support through extension methods
- Legacy types (NumberWithLogs, ValidationResult) maintained for backwards compatibility

## Monad Laws

The library respects the three monad laws for all implementations:

1. **Left Identity**: `return a >>= f ≡ f a`
2. **Right Identity**: `m >>= return ≡ m`
3. **Associativity**: `(m >>= f) >>= g ≡ m >>= (x -> f x >>= g)`

These laws are verified in the MonadLawsTests.cs file.

## Extension and Customization

To add a new monad type:
1. Create a new file in the Monads/ project
2. Implement `Map`, `Bind`, `Select`, and `SelectMany` methods
3. Add async extensions in `AsyncExtensions.cs`
4. Create a scenario in Monads.Cli/ to demonstrate usage
5. Add unit tests including monad law verification
6. Update Program.cs menu to include the new scenario

## Related Resources

- [Monads in C#: A Rigorous Exploration with Practical Examples](https://www.linkedin.com/pulse/monads-c-rigorous-exploration-practical-examples-philippe-matray--gm7qe/)
- [The Absolute Best Intro to Monads For Software Engineers](https://www.youtube.com/watch?v=C2w45qRc3aU)
- [Monad (functional programming) on Wikipedia](https://en.wikipedia.org/wiki/Monad_(functional_programming))
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)
