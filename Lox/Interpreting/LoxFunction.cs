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
public class LoxFunction(
    IReadOnlyList<Token> parameters,
    IReadOnlyList<Stmt?> body,
    Environment closure,
    string? name = null) : ILoxCallable
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
            return exception.Value;
        }

        return null;
    }

    public override string ToString() => name is not null
        ? $"<fn {name}>"
        : "<anonymous fn>";
}
