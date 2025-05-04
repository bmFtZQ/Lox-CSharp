namespace Lox.Interpreting;

public class LoxClass(
    string name,
    LoxClass? superclass,
    Dictionary<string, LoxFunction> methods) : ILoxCallable
{
    public string Name { get; } = name;
    public LoxClass? SuperClass { get; } = superclass;
    public Dictionary<string, LoxFunction> Methods { get; } = methods;

    public int Arity => FindMethod("init")?.Arity ?? 0;

    /// <summary>
    /// Construct a new instance of this class.
    /// </summary>
    /// <param name="interpreter">The interpreter to use.</param>
    /// <param name="arguments">The initializer arguments to use.</param>
    /// <returns>A new Lox instance of the Lox class.</returns>
    public object Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        var instance = new LoxInstance(this);
        var ctor = FindMethod("init");
        ctor?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    /// <summary>
    /// Find specified method on the class.
    /// </summary>
    /// <param name="name">The name of the method to find.</param>
    /// <returns>The method if exists, null otherwise.</returns>
    public LoxFunction? FindMethod(string name)
    {
        return Methods.GetValueOrDefault(name) ?? SuperClass?.FindMethod(name);
    }

    public override string ToString() => Name;
}
