namespace Lox.Tokens;

/// <summary>
/// Class to tokenize Lox source code.
/// </summary>
/// <param name="source">The source code to tokenize.</param>
public class Scanner(string source)
{
    private readonly List<Token> _tokens = [];

    // Current cursor positions.
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private bool IsAtEnd => _current >= source.Length;

    // String representations of keyword tokens.
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "and", TokenType.And },
        { "class", TokenType.Class },
        { "else", TokenType.Else },
        { "false", TokenType.False },
        { "for", TokenType.For },
        { "fun", TokenType.Fun },
        { "if", TokenType.If },
        { "nil", TokenType.Nil },
        { "or", TokenType.Or },
        { "print", TokenType.Print },
        { "return", TokenType.Return },
        { "super", TokenType.Super },
        { "this", TokenType.This },
        { "true", TokenType.True },
        { "var", TokenType.Var },
        { "while", TokenType.While }
    };

    /// <summary>
    /// Tokenize Lox source code.
    /// </summary>
    /// <returns>All the tokens parsed from the source.</returns>
    public IEnumerable<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    /// <summary>
    /// Scan a single token, and advance cursor forwards.
    /// </summary>
    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            // Non-ambiguous single-char tokens.
            case '(': AddToken(TokenType.LeftParenthesis); break;
            case ')': AddToken(TokenType.RightParenthesis); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break;

            // Ambiguous dual-char tokens.
            case '!': AddToken(MatchNext('=') ? TokenType.BangEqual : TokenType.Bang); break;
            case '=': AddToken(MatchNext('=') ? TokenType.EqualEqual : TokenType.Equal); break;
            case '<': AddToken(MatchNext('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
            case '>': AddToken(MatchNext('=') ? TokenType.LessEqual : TokenType.Less); break;

            // Comment or division.
            case '/':
                if (MatchNext('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd) Advance();
                }
                else AddToken(TokenType.Slash);

                break;

            // Strings.
            case '"':
                String();
                break;

            // New lines increment line counter.
            case '\n':
                _line++;
                break;

            default:
                if (char.IsWhiteSpace(c))
                {
                    // Ignore whitespace other than new lines.
                }
                else if (char.IsDigit(c))
                {
                    // Parse a single number token.
                    Number();
                }
                else if (IsAlpha(c))
                {
                    // Parse a single identifier or keyword token.
                    Identifier();
                }
                else
                {
                    // Unknown character.
                    Program.Error(_line, "Unexpected character.");
                }

                break;
        }
    }

    /// <summary>
    /// Get current char and advance cursor by one.
    /// </summary>
    /// <returns>The char the cursor currently points to.</returns>
    private char Advance() => source[_current++];

    /// <summary>
    /// Add token to token list, getting the text range from the current cursor
    /// positions.
    /// </summary>
    /// <param name="type">The type of token to add.</param>
    /// <param name="literal">The literal value for the token.</param>
    private void AddToken(TokenType type, object? literal = null)
    {
        var text = source[_start.._current];
        _tokens.Add(new(type, text, literal, _line));
    }

    private bool MatchNext(char expected)
    {
        if (IsAtEnd) return false;
        if (source[_current] != expected) return false;
        _current++;
        return true;
    }

    /// <summary>
    /// Peek at the current char + an optional offset.
    /// </summary>
    /// <param name="offset">An optional offset from current.</param>
    /// <returns>
    /// The char that current + offset points to, or null char if out of bounds.
    /// </returns>
    private char Peek(int offset = 0) => _current + offset >= source.Length
        ? '\0'
        : source[_current + offset];

    /// <summary>
    /// Parse and append a single string token, capturing its value.
    /// </summary>
    private void String()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd)
        {
            Program.Error(_line, "Unterminated string.");
            return;
        }

        Advance();

        var value = source[(_start + 1)..(_current - 1)];
        AddToken(TokenType.String, value);
    }

    /// <summary>
    /// Parse and append a single number token, capturing its value.
    /// </summary>
    private void Number()
    {
        while (char.IsDigit(Peek())) Advance();

        if (Peek() == '.' && char.IsDigit(Peek(1)))
        {
            Advance();

            while (char.IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.Number, double.Parse(source[_start.._current]));
    }

    /// <summary>
    /// Test whether a char is a letter or underscore.
    /// </summary>
    /// <param name="c">The char to test.</param>
    /// <returns>True if c is a letter or underscore, false otherwise.</returns>
    private static bool IsAlpha(char c) => char.IsLetter(c) || c == '_';

    /// <summary>
    /// Test whether a char is a letter, digit, or underscore.
    /// </summary>
    /// <param name="c">The char to test.</param>
    /// <returns>
    /// True if c is a letter, digit, or underscore, false otherwise.
    /// </returns>
    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsDigit(c);

    /// <summary>
    /// Parse and append a single identifier or keyword token.
    /// </summary>
    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = source[_start.._current];
        AddToken(Keywords.GetValueOrDefault(text, TokenType.Identifier));
    }
}
