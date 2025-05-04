namespace Lox.Interpreting.NativeCallables;

public class ClockCallable : ILoxCallable
{
    public static readonly ClockCallable Instance = new();

    public int Arity => 0;

    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
    }

    public override string ToString() => "<native fn>";
}
