namespace Lox.Interpreting.Builtins;

public class LoxString
{
    private readonly Interpreter _interpreter;
    public LoxClass Class { get; }

    public LoxString(Interpreter interpreter)
    {
        _interpreter = interpreter;
        Class = new LoxClass("String", staticMethods: new()
        {
            { "length", new NativeMethod(Length, 1) },
            { "charAt", new NativeMethod(CharAt, 2) },
            { "charCodeAt", new NativeMethod(CharCodeAt, 2) }
        });
    }

    private static object Length(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [string str])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return (double)str.Length;
    }

    private static object CharAt(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [string str, double index])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return str.Substring((int)index, 1);
    }

    private static object CharCodeAt(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [string str, double index])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return (double)str[(int)index];
    }
}
