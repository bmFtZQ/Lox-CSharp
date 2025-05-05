namespace Lox.Interpreting.Builtins;

public class LoxGlobalFunctions
{
    private readonly Interpreter _interpreter;
    public IReadOnlyList<(string, ILoxCallable)> Functions { get; }

    public LoxGlobalFunctions(Interpreter interpreter)
    {
        _interpreter = interpreter;
        Functions =
        [
            ("clock", new NativeFunction(Clock)),
            ("string", new NativeFunction(String, 1)),
            ("number", new NativeFunction(Number, 1)),
            ("typeOf", new NativeFunction(TypeOf, 1)),
            ("is", new NativeFunction(Is, 2)),
            ("fields", new NativeFunction(Fields, 1))
        ];
    }

    private static object Clock(IReadOnlyList<object?> args)
    {
        if (args is not [])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
    }

    private object String(IReadOnlyList<object?> args)
    {
        if (args is not [var value])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return _interpreter.Stringify(value);
    }

    private static object? Number(IReadOnlyList<object?> args)
    {
        if (args is not [var value])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        try
        {
            return Convert.ToDouble(value);
        }
        catch
        {
            return null;
        }
    }

    private static object TypeOf(IReadOnlyList<object?> args)
    {
        if (args is not [var value])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        return value switch
        {
            null => "nil",
            bool => "boolean",
            string => "string",
            double => "number",
            LoxClass => "class",
            LoxInstance => "instance",
            LoxFunction => "function",
            _ => "???"
        };
    }

    private static object Is(IReadOnlyList<object?> args)
    {
        if (args is not [var value, var type])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        switch (value)
        {
            case null when type is "nil":
            case bool when type is "boolean":
            case string when type is "string":
            case double when type is "number":
            case LoxClass when type is "class":
            case LoxInstance when type is "instance":
            case LoxFunction when type is "function":
                return true;
        }

        if (value is LoxInstance instance && type is LoxClass classType)
        {
            for (var cls = instance.Class; cls is not null; cls = cls.SuperClass)
            {
                if (cls == classType) return true;
            }
        }

        return false;
    }

    private object Fields(IReadOnlyList<object?> args)
    {
        if (args is not [LoxInstance instance])
        {
            throw new ArgumentException("Invalid argument types.", nameof(args));
        }

        if (_interpreter.Globals.Get("Array") is not LoxClass arrayClass)
        {
            throw new RunTimeException(null, "Array is not defined in global scope.");
        }

        var fields = instance.Fields
            .Select(kv => new LoxInstance(arrayClass) { Data = (object?[]) [kv.Key, kv.Value] });

        return new LoxInstance(arrayClass) { Data = fields };
    }
}
