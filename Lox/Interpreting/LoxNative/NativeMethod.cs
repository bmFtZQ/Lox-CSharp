namespace Lox.Interpreting.LoxNative;

public class NativeMethod(Delegate function, LoxInstance? self = null) : ILoxMethod
{
    public int Arity { get; } = function.Method.GetParameters().Length - 1;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        if (self is null)
        {
            throw new RunTimeException(null, "Attempt to call native method as function.");
        }

        try
        {
            return function.DynamicInvoke([self, ..arguments]);
        }
        catch (ArgumentException)
        {
            throw new RunTimeException(null, "Incorrect argument types for native method.");
        }
    }

    public ILoxMethod Bind(LoxInstance instance)
    {
        return new NativeMethod(function, instance);
    }

    public override string ToString() => "<native fn>";
}
