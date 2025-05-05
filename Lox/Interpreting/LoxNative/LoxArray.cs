namespace Lox.Interpreting.LoxNative;

public static class LoxArray
{
    public static LoxClass MakeClass(Interpreter interpreter)
    {
        return new LoxClass("Array", methods: new Dictionary<string, ILoxMethod>
        {
            {
                "init",
                new NativeMethod((t, p) =>
                {
                    if (p is [double length])
                    {
                        t.Data = new object?[(int)length];
                    }
                    else
                    {
                        throw new ArgumentException(null);
                    }
                    return t;
                }, 1)
            },
            {
                "get",
                new NativeMethod((t, p) =>
                {
                    if (p is not [double index])
                    {
                        throw new ArgumentException(null);
                    }

                    var array = t.Data as object?[];
                    return array?[(int)index];
                }, 1)
            },
            {
                "set",
                new NativeMethod((t, p) =>
                {
                    if (p is not [double index, var value])
                    {
                        throw new ArgumentException(null);
                    }

                    var array = t.Data as object?[];
                    return array![(int)index] = value;
                }, 2)
            },
            {
                "length",
                new NativeMethod((t, _) => (double?)(t.Data as object?[])?.Length)
            },
            {
                "foreach",
                new NativeMethod((t, p) =>
                {
                    if (p is not [ILoxCallable function])
                    {
                        throw new ArgumentException(null);
                    }

                    var array = t.Data as object?[];
                    foreach (var (obj, i) in array?.Select((x, i) => (x, i)) ?? [])
                    {
                        function.Call(interpreter, [obj, (double)i]);
                    }

                    return null;
                }, 1)
            },
            {
                "toString",
                new NativeMethod((t, _) =>
                {
                    var array = t.Data as object?[];
                    var str = array!.Select(interpreter.Stringify);
                    return $"[{string.Join(", ", str)}]";
                })
            }
        });
    }
}
