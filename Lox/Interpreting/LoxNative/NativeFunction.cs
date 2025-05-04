using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Lox.Interpreting.LoxNative;

public class NativeFunction(Delegate function) : ILoxCallable
{
    public int Arity { get; } = function.Method.GetParameters().Length;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        try
        {
            return function.DynamicInvoke([..arguments]);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException!;
        }
        catch (ArgumentException)
        {
            throw new RunTimeException(null, "Incorrect argument types for native method.");
        }
    }

    public override string ToString() => "<native fn>";
}
