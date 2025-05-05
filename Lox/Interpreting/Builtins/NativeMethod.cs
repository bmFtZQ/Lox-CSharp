namespace Lox.Interpreting.Builtins;

public class NativeMethod(
    Func<LoxInstance, IReadOnlyList<object?>, object?> function,
    int arity = 0,
    LoxInstance? self = null) : ILoxMethod
{
    public int Arity { get; } = arity;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        if (self is null)
        {
            throw new RunTimeException(null, "Native method has no 'instance'.");
        }

        try
        {
            return function(self, arguments);
        }
        catch (ArgumentException)
        {
            throw new RunTimeException(null, "Incorrect argument types for native method.");
        }
    }

    public ILoxMethod Bind(LoxInstance instance)
    {
        return new NativeMethod(function, Arity, instance);
    }

    public override string ToString() => "<native fn>";
}
