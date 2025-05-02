namespace Lox.Tokens;

public class Token(TokenType type, string lexeme, object? literal, int line)
{
    public TokenType Type { get; } = type;
    public string Lexeme { get; } = lexeme;
    public object? Literal { get; } = literal;
    public int Line { get; } = line;

    public override string ToString()
    {
        return $"{Type} {Lexeme} {Literal}";
    }
}
