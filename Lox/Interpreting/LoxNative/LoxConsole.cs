namespace Lox.Interpreting.LoxNative;

public static class LoxConsole
{
    public static LoxClass MakeClass(Interpreter interpreter)
    {
        return new LoxClass("Console", staticMethods: new Dictionary<string, ILoxMethod>
        {
            {
                "readLine",
                new NativeMethod((_, _) => Console.ReadLine())
            },
            {
                "writeLine",
                new NativeMethod((_, p) =>
                {
                    Console.WriteLine(interpreter.Stringify(p[0]));
                    return null;
                }, 1)
            },
            {
                "write",
                new NativeMethod((_, p) =>
                {
                    Console.Write(interpreter.Stringify(p[0]));
                    return null;
                }, 1)
            }
        });
    }
}
