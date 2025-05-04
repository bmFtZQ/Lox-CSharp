using Lox.Tokens;

namespace Lox.Interpreting;

public class LoxInstance(LoxClass cls)
{
    private readonly Dictionary<string, object?> _fields = [];

    /// <summary>
    /// Get a property value from this instance.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The value of the property specified.</returns>
    /// <exception cref="RunTimeException">
    /// Thrown if the specified property does not exist.
    /// </exception>
    public object? Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        return cls.FindMethod(name.Lexeme)?.Bind(this) ??
               throw new RunTimeException(name, $"Undefined property '{name.Lexeme}'.");
    }

    /// <summary>
    /// Set a property value on this instance.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property to.</param>
    public void Set(Token name, object? value)
    {
        _fields[name.Lexeme] = value;
    }

    public override string ToString() => $"<{cls.Name} instance>";
}
