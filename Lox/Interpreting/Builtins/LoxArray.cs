namespace Lox.Interpreting.Builtins;

public class LoxArray
{
    private readonly Interpreter _interpreter;
    public LoxClass Class { get; }

    public LoxArray(Interpreter interpreter)
    {
        _interpreter = interpreter;
        Class = new LoxClass("Array", methods: new()
        {
            { "init", new NativeMethod(Init, 1) },
            { "length", new NativeMethod(Length) },
            { "foreach", new NativeMethod(ForEach, 1) },
            { "toString", new NativeMethod(Method_ToString) }
        }, makeInstance: c => new LoxArrayInstance(c));
    }

    private static void ThrowIfNotInstance(LoxInstance instance, out LoxArrayInstance array)
    {
        array = instance as LoxArrayInstance ?? throw new RunTimeException(null, "");
    }

    private static object Init(LoxInstance self, IReadOnlyList<object?> args)
    {
        ThrowIfNotInstance(self, out var arr);
        if (args is not [double length])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        arr.Array = [];
        arr.Array.AddRange(new object?[(int)length]);

        return arr;
    }

    private static object Length(LoxInstance self, IReadOnlyList<object?> args)
    {
        ThrowIfNotInstance(self, out var arr);
        if (args is not [])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return (double)arr.Array.Count;
    }

    private object? ForEach(LoxInstance self, IReadOnlyList<object?> args)
    {
        ThrowIfNotInstance(self, out var arr);
        if (args is not [ILoxCallable function])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        foreach (var (obj, i) in arr.Array.Select((x, i) => (x, i)))
        {
            function.Call(_interpreter, [obj, (double)i]);
        }

        return null;
    }

    private object Method_ToString(LoxInstance self, IReadOnlyList<object?> args)
    {
        ThrowIfNotInstance(self, out var arr);
        if (args is not [])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        var str = arr.Array.Select(_interpreter.Stringify);
        return $"[{string.Join(", ", str)}]";
    }
}
