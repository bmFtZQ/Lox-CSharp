namespace Lox.Interpreting.LoxNative;

public static class LoxConsole
{
    public static LoxClass MakeClass(Interpreter interpreter)
    {
        return new LoxClass("Console", staticMethods: new Dictionary<string, ILoxMethod>
        {
            {
                "readLine",
                new NativeMethod((LoxClass t) => Console.ReadLine())
            },
            {
                "writeLine",
                new NativeMethod((LoxClass t, object? obj) => Console.WriteLine(interpreter.Stringify(obj)))
            },
            {
                "write",
                new NativeMethod((LoxClass t, object? obj) => Console.Write(interpreter.Stringify(obj)))
            }
        });
    }
}
