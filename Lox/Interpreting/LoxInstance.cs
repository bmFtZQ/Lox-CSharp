using Lox.Tokens;

namespace Lox.Interpreting;

public class LoxInstance(LoxClass? cls = null)
{
    public LoxClass? Class { get; protected init; } = cls;
    public Dictionary<string, object?> Fields { get; } = [];

    /// <summary>
    /// Arbitrary data inaccessible to Lox, can be used to store native data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Get a property value from this instance.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The value of the property specified.</returns>
    /// <exception cref="RunTimeException">
    /// Thrown if the specified property does not exist.
    /// </exception>
    public object? Get(Token name) => Get(name.Lexeme, name);

    public object? Get(string name, Token? token = null)
    {
        if (Fields.TryGetValue(name, out var value))
        {
            return value;
        }

        return Class?.FindMethod(name)?.Bind(this) ??
               throw new RunTimeException(token, $"Undefined property '{name}'.");
    }

    /// <summary>
    /// Set a property value on this instance.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property to.</param>
    public void Set(Token name, object? value)
    {
        Fields[name.Lexeme] = value;
    }

    public void Set(string name, object? value)
    {
        Fields[name] = value;
    }

    public override string ToString() => $"<{Class?.Name} instance>";
}
