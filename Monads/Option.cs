namespace Monads;

/// <summary>
/// Option monad for representing optional values (Maybe pattern).
/// Eliminates null reference errors by making absence explicit.
/// </summary>
/// <typeparam name="T">The type of the optional value</typeparam>
public abstract record Option<T>
{
    /// <summary>
    /// Indicates whether this option contains a value.
    /// </summary>
    public abstract bool IsSome { get; }

    /// <summary>
    /// Indicates whether this option is empty.
    /// </summary>
    public bool IsNone => !IsSome;

    /// <summary>
    /// Creates an Option with a value (Some).
    /// </summary>
    public static Option<T> Some(T value) => new SomeOption(value);

    /// <summary>
    /// Creates an empty Option (None).
    /// </summary>
    public static Option<T> None() => new NoneOption();

    /// <summary>
    /// Creates an Option from a nullable value (handled via extension methods).
    /// </summary>
    internal static Option<T> FromValue(T value)
        => value is not null ? Some(value) : None();

    /// <summary>
    /// Maps a function over the option value if present.
    /// </summary>
    public abstract Option<TResult> Map<TResult>(Func<T, TResult> mapper);

    /// <summary>
    /// Monadic bind operation (flatMap).
    /// </summary>
    public abstract Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder);

    /// <summary>
    /// Applies a function wrapped in an Option to this Option's value.
    /// </summary>
    public abstract Option<TResult> Apply<TResult>(Option<Func<T, TResult>> optionFunc);

    /// <summary>
    /// Returns the value if Some, otherwise returns the provided default value.
    /// </summary>
    public abstract T GetValueOrDefault(T defaultValue);

    /// <summary>
    /// Returns the value if Some, otherwise returns the result of the provided function.
    /// </summary>
    public abstract T GetValueOrDefault(Func<T> defaultValueFactory);

    /// <summary>
    /// Executes an action if the option contains a value.
    /// </summary>
    public abstract Option<T> Do(Action<T> action);

    /// <summary>
    /// Executes the appropriate action based on whether the option is Some or None.
    /// </summary>
    public abstract void Match(Action<T> onSome, Action onNone);

    /// <summary>
    /// Returns a result based on whether the option is Some or None.
    /// </summary>
    public abstract TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone);

    /// <summary>
    /// Filters the option based on a predicate.
    /// </summary>
    public abstract Option<T> Where(Func<T, bool> predicate);

    /// <summary>
    /// LINQ Select support (synonym for Map).
    /// </summary>
    public Option<TResult> Select<TResult>(Func<T, TResult> selector)
        => Map(selector);

    /// <summary>
    /// LINQ SelectMany support (synonym for Bind).
    /// </summary>
    public Option<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Option<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> projector)
        => Bind(x => selector(x).Map(y => projector(x, y)));

    /// <summary>
    /// Converts the Option to an enumerable for iteration.
    /// </summary>
    public abstract IEnumerable<T> AsEnumerable();

    /// <summary>
    /// Converts the Option to an enumerable (0 or 1 element).
    /// </summary>
    public abstract IEnumerable<T> ToEnumerable();

    private sealed record SomeOption(T Value) : Option<T>
    {
        public override bool IsSome => true;

        public override Option<TResult> Map<TResult>(Func<T, TResult> mapper)
            => Option<TResult>.Some(mapper(Value));

        public override Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)
            => binder(Value);

        public override Option<TResult> Apply<TResult>(Option<Func<T, TResult>> optionFunc)
            => optionFunc.Map(f => f(Value));

        public override T GetValueOrDefault(T defaultValue)
            => Value;

        public override T GetValueOrDefault(Func<T> defaultValueFactory)
            => Value;

        public override Option<T> Do(Action<T> action)
        {
            action(Value);
            return this;
        }

        public override void Match(Action<T> onSome, Action onNone)
            => onSome(Value);

        public override TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
            => onSome(Value);

        public override Option<T> Where(Func<T, bool> predicate)
            => predicate(Value) ? this : None();

        public override IEnumerable<T> AsEnumerable()
        {
            yield return Value;
        }

        public override IEnumerable<T> ToEnumerable()
        {
            yield return Value;
        }
    }

    private sealed record NoneOption() : Option<T>
    {
        public override bool IsSome => false;

        public override Option<TResult> Map<TResult>(Func<T, TResult> mapper)
            => Option<TResult>.None();

        public override Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)
            => Option<TResult>.None();

        public override Option<TResult> Apply<TResult>(Option<Func<T, TResult>> optionFunc)
            => Option<TResult>.None();

        public override T GetValueOrDefault(T defaultValue)
            => defaultValue;

        public override T GetValueOrDefault(Func<T> defaultValueFactory)
            => defaultValueFactory();

        public override Option<T> Do(Action<T> action)
            => this;

        public override void Match(Action<T> onSome, Action onNone)
            => onNone();

        public override TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
            => onNone();

        public override Option<T> Where(Func<T, bool> predicate)
            => this;

        public override IEnumerable<T> AsEnumerable()
        {
            yield break;
        }

        public override IEnumerable<T> ToEnumerable()
        {
            yield break;
        }
    }
}

/// <summary>
/// Extension methods for Option monad.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Converts a nullable reference type to an Option.
    /// </summary>
    public static Option<T> ToOption<T>(this T? value) where T : class
        => value is not null ? Option<T>.Some(value) : Option<T>.None();

    /// <summary>
    /// Converts a nullable value type to an Option.
    /// </summary>
    public static Option<T> ToOption<T>(this T? value) where T : struct
        => value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None();

    /// <summary>
    /// Converts an Option to a nullable value type.
    /// </summary>
    public static T? ToNullable<T>(this Option<T> option) where T : struct
        => option.Match(
            onSome: value => (T?)value,
            onNone: () => null);

    /// <summary>
    /// Flattens a nested Option.
    /// </summary>
    public static Option<T> Flatten<T>(this Option<Option<T>> option)
        => option.Bind(x => x);

    /// <summary>
    /// Combines two Options using a combining function.
    /// </summary>
    public static Option<TResult> Zip<T1, T2, TResult>(
        this Option<T1> option1,
        Option<T2> option2,
        Func<T1, T2, TResult> combiner)
        => option1.Bind(v1 => option2.Map(v2 => combiner(v1, v2)));
}