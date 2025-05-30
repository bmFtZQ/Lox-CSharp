namespace Lox.Interpreting;

public class LoxClass : LoxInstance, ILoxCallable
{
    public LoxClass(string name,
        LoxClass? superclass = null,
        Dictionary<string, ILoxMethod>? methods = null,
        Dictionary<string, ILoxMethod>? staticMethods = null,
        Func<LoxClass, LoxInstance>? makeInstance = null)
    {
        Name = name;
        SuperClass = superclass;
        Methods = methods ?? [];
        _makeInstanceFunction = makeInstance ?? (c => new LoxInstance(c));

        if (staticMethods is not null)
        {
            var meta = new LoxClass($"__{name}_metaclass", superclass?.Class, staticMethods);
            Class = meta;
        }
    }

    private readonly Func<LoxClass, LoxInstance> _makeInstanceFunction;
    public string Name { get; }
    public LoxClass? SuperClass { get; }
    public Dictionary<string, ILoxMethod> Methods { get; }

    public int Arity => FindMethod("init")?.Arity ?? 0;

    /// <summary>
    /// Construct a new instance of this class.
    /// </summary>
    /// <param name="interpreter">The interpreter to use.</param>
    /// <param name="arguments">The initializer arguments to use.</param>
    /// <returns>A new Lox instance of the Lox class.</returns>
    public object Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        var instance = MakeInstance();
        var ctor = FindMethod("init");
        ctor?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    /// <summary>
    /// Find specified method on the class.
    /// </summary>
    /// <param name="name">The name of the method to find.</param>
    /// <returns>The method if exists, null otherwise.</returns>
    public ILoxMethod? FindMethod(string name)
    {
        return Methods.GetValueOrDefault(name) ?? SuperClass?.FindMethod(name);
    }

    public LoxInstance MakeInstance()
    {
        return _makeInstanceFunction(this);
    }

    public override string ToString() => $"{Name}";
}
