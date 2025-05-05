namespace Lox.Interpreting.Builtins;

public class LoxMath
{
    private readonly Interpreter _interpreter;
    public LoxClass Class { get; }

    public LoxMath(Interpreter interpreter)
    {
        _interpreter = interpreter;
        Class = new LoxClass("Math", staticMethods: new()
        {
            { "mod", new NativeMethod(Mod, 2) },
            { "round", new NativeMethod(Round, 2) },
        });
    }

    private static object Mod(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [double a, double b])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return a % b;
    }

    private static object Round(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [double a, double b])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return Math.Round(a, (int)b);
    }
}
