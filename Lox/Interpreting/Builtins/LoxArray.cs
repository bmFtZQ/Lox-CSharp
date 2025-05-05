namespace Lox.Interpreting.Builtins;

public class LoxArray
{
    private readonly Interpreter _interpreter;
    public LoxClass Class { get; }

    public LoxArray(Interpreter interpreter)
    {
        _interpreter = interpreter;
        Class = new LoxClass("Math", staticMethods: new()
        {
            { "init", new NativeMethod(Init, 2) },
            { "get", new NativeMethod(Get, 1) },
            { "set", new NativeMethod(Set, 2) },
            { "length", new NativeMethod(Length) },
            { "foreach", new NativeMethod(ForEach, 1) },
            { "toString", new NativeMethod(Method_ToString) }
        });
    }

    private static object Init(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [double length])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        self.Data = new object?[(int)length];
        return self;
    }

    private static object? Get(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [double index])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return (self.Data as object?[])?[(int)index];
    }

    private static object? Set(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [double index, var value])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return (self.Data as object?[])![(int)index] = value;
    }

    private static object? Length(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return (double?)(self.Data as object?[])?.Length;
    }

    private object? ForEach(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [ILoxCallable function])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        var array = self.Data as object?[];
        foreach (var (obj, i) in array?.Select((x, i) => (x, i)) ?? [])
        {
            function.Call(_interpreter, [obj, (double)i]);
        }

        return null;
    }

    private object Method_ToString(LoxInstance self, IReadOnlyList<object?> args)
    {
        if (args is not [])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        var array = self.Data as object?[];
        var str = array!.Select(_interpreter.Stringify);
        return $"[{string.Join(", ", str)}]";
    }
}
