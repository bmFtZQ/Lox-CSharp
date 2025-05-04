namespace Lox.Interpreting.LoxNative;

public static class GlobalFunctions
{
    public static IEnumerable<(string, ILoxCallable)> MakeFunctions(Interpreter interpreter) =>
    [
        // Get the current time in milliseconds.
        ("clock", new NativeFunction(() => DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0)),

        // Convert value into a string.
        ("string", new NativeFunction(interpreter.Stringify)),

        // Convert value into a number, returns nil if not possible.
        ("number", new NativeFunction(double? (object? value) =>
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return null;
            }
        })),

        // Get the type code of a value.
        ("typeOf", new NativeFunction(string (object? value) => value switch
        {
            null => "nil",
            bool => "boolean",
            string => "string",
            double => "number",
            LoxClass => "class",
            LoxInstance => "instance",
            LoxFunction => "function",
            _ => "???"
        })),

        // Test that a value is of a certain type, either by passing a class to
        // test by, or a type code string.
        ("is", new NativeFunction(bool (object? value, object? type) =>
        {
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
                var classB = value is LoxClass
                    ? classType.Class
                    : classType;

                for (var cls = instance.Class;; cls = cls.SuperClass)
                {
                    if (cls is null) return false;
                    if (cls == classB) return true;
                }
            }

            return false;
        }))
    ];
}
