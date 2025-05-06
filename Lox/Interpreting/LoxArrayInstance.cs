using Lox.Tokens;

namespace Lox.Interpreting;

public class LoxArrayInstance(LoxClass cls, IEnumerable<object?>? elements = null) : LoxInstance(cls)
{
    public List<object?> Array { get; set; } = [..elements ?? []];

    public object? Get(double index, Token? token = null)
    {
        try
        {
            return Array[(int)index];
        }
        catch (IndexOutOfRangeException)
        {
            throw new RunTimeException(token, "Array index out of bounds.");
        }
    }

    public void Set(double index, object? value, Token? token = null)
    {
        try
        {
            Array[(int)index] = value;
        }
        catch (IndexOutOfRangeException)
        {
            throw new RunTimeException(token, "Array index out of bounds.");
        }
    }
}
