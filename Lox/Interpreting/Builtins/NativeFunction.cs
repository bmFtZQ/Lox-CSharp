namespace Lox.Interpreting.Builtins;

public class NativeFunction(
    Func<IReadOnlyList<object?>, object?> function,
    int arity = 0) : ILoxCallable
{
    public int Arity { get; } = arity;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        try
        {
            return function(arguments);
        }
        catch (ArgumentException)
        {
            throw new RunTimeException(null, "Incorrect argument types for native method.");
        }
    }

    public override string ToString() => "<native fn>";
}
