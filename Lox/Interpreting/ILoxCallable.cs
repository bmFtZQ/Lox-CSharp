namespace Lox.Interpreting;

/// <summary>
/// Represents any object that can be called in Lox.
/// </summary>
public interface ILoxCallable
{
    /// <summary>
    /// The number of arguments the function requires.
    /// </summary>
    public int Arity { get; }

    /// <summary>
    /// Call the function and execute the statements within.
    /// </summary>
    /// <param name="interpreter">The interpreter to use.</param>
    /// <param name="arguments">The arguments to pass into the function, count
    /// must match the function's arity.</param>
    /// <returns>
    /// The value returned from the function, or null if the function did not
    /// return a value.
    /// </returns>
    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments);
}

/// <summary>
/// Methods are callables that can be bound to an instance.
/// </summary>
public interface ILoxMethod : ILoxCallable
{
    ILoxMethod Bind(LoxInstance instance);
}
