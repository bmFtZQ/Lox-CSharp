using Lox.Parsing;

namespace Lox.Interpreting;

/// <summary>
/// Represents a user-defined Lox function callable.
/// </summary>
/// <param name="declaration">The function definition to use.</param>
public class LoxFunction(FunctionStmt declaration) : ILoxCallable
{
    public int Arity => declaration.Parameters.Count;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        var environment = new Environment(interpreter.Globals);

        foreach (var (param, arg) in declaration.Parameters.Zip(arguments))
        {
            environment.Define(param.Lexeme, arg);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (ReturnException exception)
        {
            return exception.Value;
        }

        return null;
    }

    public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
}
