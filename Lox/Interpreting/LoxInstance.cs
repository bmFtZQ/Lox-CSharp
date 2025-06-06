using Lox.Tokens;

namespace Lox.Interpreting;

public class LoxInstance(LoxClass? cls = null)
{
    public LoxClass? Class { get; protected init; } = cls;
    public Dictionary<string, object?> Fields { get; } = [];

    /// <summary>
    /// Get a property value from this instance.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <param name="token">The token that identifies this get if an error
    /// occurs.</param>
    /// <returns>The value of the property specified.</returns>
    /// <exception cref="RunTimeException">
    /// Thrown if the specified property does not exist.
    /// </exception>
    public object? Get(string name, Token? token = null)
    {
        if (Fields.TryGetValue(name, out var value))
        {
            return value;
        }

        return GetMethod(name) ?? throw new RunTimeException(token, $"Undefined property '{name}'.");
    }

    /// <summary>
    /// Set a property value on this instance.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property to.</param>
    /// <param name="token">Token that identifies this set if error
    /// occurs.</param>
    public void Set(string name, object? value, Token? token = null)
    {
        Fields[name] = value;
    }

    /// <summary>
    /// Finds and binds the specified method for this class.
    /// </summary>
    /// <param name="name">The name of the method to bind.</param>
    /// <returns>The bound method if exists, null otherwise.</returns>
    public ILoxMethod? GetMethod(string name) => Class?.FindMethod(name)?.Bind(this);

    public override string ToString() => $"{Class?.Name} instance";
}
