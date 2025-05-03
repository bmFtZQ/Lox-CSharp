using Lox.Tokens;

namespace Lox.Interpreting;

public class Environment(Environment? enclosing = null)
{
    private readonly Dictionary<string, object?> _values = [];

    /// <summary>
    /// Define a new variable in the environment scope.
    /// </summary>
    /// <param name="name">The name of the variable to define.</param>
    /// <param name="value">The initial value of the variable.</param>
    public void Define(string name, object? value = null)
    {
        _values[name] = value;
    }

    /// <summary>
    /// Get an existing variable from the environment.
    /// </summary>
    /// <param name="name">The variable to get.</param>
    /// <returns></returns>
    /// <exception cref="RunTimeException">
    /// Thrown if attempted to access a variable that doesn't exist.
    /// </exception>
    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (enclosing is not null)
        {
            return enclosing.Get(name);
        }

        throw new RunTimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }

    /// <summary>
    /// Assign a value that already exists within the environment.
    /// </summary>
    /// <param name="name">The name of the variable to assign.</param>
    /// <param name="value">The value to assign.</param>
    /// <exception cref="RunTimeException">Thrown if variable does not already
    /// exist within environment.</exception>
    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (enclosing is not null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RunTimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
