using Lox.Parsing;
using Lox.Tokens;

namespace Lox.Interpreting;

/// <summary>
/// Represents a user-defined Lox function callable.
/// </summary>
/// <param name="parameters">The function parameters.</param>
/// <param name="body">The function body statements.</param>
/// <param name="closure">The environment for the function to use.</param>
/// <param name="name">Name of the function.</param>
/// <param name="isInitializer">If the function is a class initializer.</param>
public class LoxFunction(
    IReadOnlyList<Token> parameters,
    IReadOnlyList<Stmt?> body,
    Environment closure,
    string? name = null,
    bool isInitializer = false) : ILoxCallable
{
    public int Arity => parameters.Count;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        var environment = new Environment(closure);

        foreach (var (param, arg) in parameters.Zip(arguments))
        {
            environment.Define(param.Lexeme, arg);
        }

        try
        {
            interpreter.ExecuteBlock(body, environment);
        }
        catch (ReturnException exception)
        {
            // Initializers always return 'this'.
            return isInitializer
                ? closure.GetAt(0, "this")
                : exception.Value;
        }

        // Initializers always return 'this'.
        return isInitializer
            ? closure.GetAt(0, "this")
            : null;
    }

    /// <summary>
    /// Bind an instance of this function to a class instance.
    /// </summary>
    /// <param name="instance">The class instance to bind to.</param>
    /// <returns>A copy of this function bound to the instance.</returns>
    public LoxFunction Bind(LoxInstance instance)
    {
        var env = new Environment(closure);
        env.Define("this", instance);
        return new LoxFunction(parameters, body, env, name, isInitializer);
    }

    public override string ToString() => name is not null
        ? $"<fn {name}>"
        : "<anonymous fn>";
}
