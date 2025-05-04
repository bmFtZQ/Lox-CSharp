namespace Lox.Interpreting.LoxNative;

public static class LoxArray
{
    public static LoxClass MakeClass(Interpreter interpreter)
    {
        return new LoxClass("Array", methods: new Dictionary<string, ILoxMethod>
        {
            {
                "init",
                new NativeMethod((LoxInstance t, double capacity) => { t.Data = new object?[(int)capacity]; })
            },
            {
                "get",
                new NativeMethod((LoxInstance t, double index) =>
                {
                    var array = t.Data as object?[];
                    return array?[(int)index];
                })
            },
            {
                "set",
                new NativeMethod((LoxInstance t, double index, object? value) =>
                {
                    var array = t.Data as object?[];
                    return array![(int)index] = value;
                })
            },
            {
                "length",
                new NativeMethod((LoxInstance t) => (double?)(t.Data as object?[])?.Length)
            },
            {
                "foreach",
                new NativeMethod((LoxInstance t, ILoxCallable function) =>
                {
                    var array = t.Data as object?[];
                    foreach (var (obj, i) in array?.Select((x, i) => (x, i)) ?? [])
                    {
                        function.Call(interpreter, [obj, (double)i]);
                    }
                })
            },
            {
                "toString",
                new NativeMethod((LoxInstance t) =>
                {
                    var array = t.Data as object?[];
                    var str = array!.Select(interpreter.Stringify);
                    return $"[{string.Join(", ", str)}]";
                })
            }
        });
    }
}
