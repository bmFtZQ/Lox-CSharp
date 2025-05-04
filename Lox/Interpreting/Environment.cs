using Lox.Tokens;

namespace Lox.Interpreting;

public class Environment(Environment? enclosing = null)
{
    private readonly Environment? _enclosing = enclosing;
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
    /// <returns>The variable value from the environment.</returns>
    /// <exception cref="RunTimeException">
    /// Thrown if attempted to access a variable that doesn't exist.
    /// </exception>
    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (_enclosing is not null)
        {
            return _enclosing.Get(name);
        }

        throw new RunTimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }

    /// <summary>
    /// Get an existing variable from specified environment.
    /// </summary>
    /// <param name="distance">The parent environment to find the variable.</param>
    /// <param name="name">The name of the variable to get.</param>
    /// <returns>The variable value from the environment.</returns>
    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)?._values[name];
    }

    /// <summary>
    /// Get a parent environment using its distance from the current.
    /// </summary>
    /// <param name="distance">How many levels to traverse up.</param>
    /// <returns>The parent environment specified by the distance.</returns>
    private Environment? Ancestor(int distance)
    {
        var env = this;
        for (var i = 0; i < distance; i++)
        {
            env = env?._enclosing;
        }

        return env;
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

        if (_enclosing is not null)
        {
            _enclosing.Assign(name, value);
            return;
        }

        throw new RunTimeException(name, $"Undefined variable '{name.Lexeme}'.");
    }

    /// <summary>
    /// Assign an existing variable from specified environment.
    /// </summary>
    /// <param name="distance">The parent environment to find the variable.</param>
    /// <param name="name">The name of the variable to assign.</param>
    /// <param name="value">The value to assign the variable.</param>
    public void AssignAt(int distance, Token name, object? value)
    {
        var env = Ancestor(distance);

        if (env is not null)
        {
            env._values[name.Lexeme] = value;
        }
    }
}
