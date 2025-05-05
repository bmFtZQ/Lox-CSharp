namespace Lox.Interpreting.Builtins;

public class LoxConsole
{
    private readonly Interpreter _interpreter;
    public LoxClass Class { get; }

    public LoxConsole(Interpreter interpreter)
    {
        _interpreter = interpreter;
        Class = new LoxClass("Console", staticMethods: new()
        {
            { "readLine", new NativeMethod(ReadLine) },
            { "writeLine", new NativeMethod(WriteLine, 1) },
            { "write", new NativeMethod(Write, 1) },
        });
    }

    private static object? ReadLine(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return Console.ReadLine();
    }

    private static object? WriteLine(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [var value])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        Console.WriteLine(value);
        return null;
    }

    private static object? Write(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [var value])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        Console.Write(value);
        return null;
    }
}
