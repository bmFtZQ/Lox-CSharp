using Lox.Tokens;

namespace Lox.Parsing;

public class Parser(IEnumerable<Token> tokens)
{
    private readonly List<Token> _tokens = [.. tokens];
    private int _current = 0;

    private bool IsAtEnd => Peek().Type == TokenType.Eof;

    /// <summary>
    /// Parse the list of tokens from a program into an AST with the list of
    /// statements.
    /// </summary>
    /// <returns>An AST populated with the list of statements.</returns>
    public IEnumerable<Stmt?> Parse()
    {
        List<Stmt?> statements = [];

        while (!IsAtEnd)
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private class ParseException : Exception;

    /// <summary>
    /// Scan for declarations, lowest precedence, will cascade to lower level
    /// statements.
    /// </summary>
    /// <returns>An AST populate with the parsed declaration.</returns>
    private Stmt? Declaration()
    {
        try
        {
            return Match(TokenType.Var)
                ? VarDeclaration()
                : Statement();
        }
        catch (ParseException)
        {
            Synchronize();
            return null;
        }
    }

    /// <summary>
    /// Scan for variable declaration statement.
    /// </summary>
    /// <returns>An AST with the parsed variable declaration.</returns>
    private VarStmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected variable name.");

        var initializer = Match(TokenType.Equal)
            ? Expression()
            : null;

        Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");
        return new VarStmt(name, initializer);
    }

    /// <summary>
    /// Scan for statements, will cascade to lower level statements.
    /// </summary>
    /// <returns>An AST populate with the parsed statement.</returns>
    private Stmt Statement()
    {
        if (Match(TokenType.Print))
        {
            return PrintStatement();
        }

        return ExpressionStatement();
    }

    /// <summary>
    /// Scan for print statement.
    /// </summary>
    /// <returns>An AST populated with the parsed statement.</returns>
    private PrintStmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new PrintStmt(value);
    }

    /// <summary>
    /// Scan for expression statement.
    /// </summary>
    /// <returns>An AST populated with the parsed statement.</returns>
    private ExpressionStmt ExpressionStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new ExpressionStmt(value);
    }

    /// <summary>
    /// Scan for expression, lowest precedence.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Expression() => Assignment();

    /// <summary>
    /// Scan for assignment expression.
    /// </summary>
    /// <returns>An AST populated with the parsed expression.</returns>
    private Expr Assignment()
    {
        var expr = Equality();

        if (Match(TokenType.Equal))
        {
            // Search for 'l-value' that can be assigned to.
            var equals = Previous();

            // Search for additional assignment expressions or navigate further
            // down the grammar.
            var value = Assignment();

            if (expr is VariableExpr varExpr)
            {
                var name = varExpr.Name;
                return new AssignExpr(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    /// <summary>
    /// Scan for equality expression, navigating further down hierarchy if no
    /// equality expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Equality()
    {
        var expr = Comparison();
        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var op = Previous();
            var right = Comparison();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Scan for comparison expression, navigating further down hierarchy if no
    /// comparison expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Comparison()
    {
        var expr = Term();
        while (Match(TokenType.GreaterEqual, TokenType.LessEqual, TokenType.Greater, TokenType.Less))
        {
            var op = Previous();
            var right = Term();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Scan for term expression (binary + or -), navigating further down
    /// hierarchy if no term expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Term()
    {
        var expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            var right = Factor();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Scan for factor (* or /) expression, navigating further down hierarchy
    /// if no factor expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Star, TokenType.Slash))
        {
            var op = Previous();
            var right = Unary();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Scan for unary (unary ! or -) expression, navigating further down
    /// hierarchy if no unary expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            var op = Previous();
            var right = Unary();
            return new UnaryExpr(op, right);
        }

        return Primary();
    }

    /// <summary>
    /// Scan for primary expressions, these are expression that can terminate
    /// and have a direct value or group other expressions together.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Primary()
    {
        if (Match(TokenType.False)) return new LiteralExpr(false);
        if (Match(TokenType.True)) return new LiteralExpr(true);
        if (Match(TokenType.Nil)) return new LiteralExpr(null);

        if (Match(TokenType.Number, TokenType.String))
        {
            return new LiteralExpr(Previous().Literal);
        }

        if (Match(TokenType.LeftParenthesis))
        {
            var expr = Expression();
            Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
            return new GroupingExpr(expr);
        }

        if (Match(TokenType.Identifier))
        {
            return new VariableExpr(Previous());
        }

        throw Error(Peek(), "Expected an expression.");
    }

    /// <summary>
    /// Synchronize parser after an error occurs within an expression.
    /// </summary>
    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd)
        {
            if (Previous().Type == TokenType.Semicolon) return;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }

    /// <summary>
    /// Check that the next token is equal to the specified token, otherwise
    /// report an error.
    /// </summary>
    /// <param name="type">The token to test against the current.</param>
    /// <param name="message">The message for a possible error.</param>
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    /// <summary>
    /// Helper function to report and return an error.
    /// </summary>
    /// <param name="token">The token that caused the error.</param>
    /// <param name="message">The message used to report the error.</param>
    /// <returns>A new ParseException.</returns>
    private static ParseException Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseException();
    }

    /// <summary>
    /// Check if any of the specified tokens are equal to the current token and
    /// advance cursor if so.
    /// </summary>
    /// <param name="types">The tokens to test against the current.</param>
    /// <returns>
    /// True if the specified tokens are equal to the current token, false if
    /// not.
    /// </returns>
    private bool Match(params IEnumerable<TokenType> types)
    {
        if (types.Any(Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Test that the token pointed to by the current cursor is equal to the
    /// specified token.
    /// </summary>
    /// <param name="type">The token to test the current against.</param>
    /// <returns>
    /// True if the specified token is equal to the current token, false if not.
    /// </returns>
    private bool Check(TokenType type)
    {
        return Peek().Type == type;
    }

    /// <summary>
    /// Advance cursor by one token and return current token.
    /// </summary>
    /// <returns>
    /// The current token before advancing the cursor position.
    /// </returns>
    private Token Advance()
    {
        if (!IsAtEnd) _current++;
        return Previous();
    }

    /// <summary>
    /// Peek at the current token yet to be consumed.
    /// </summary>
    /// <returns>The token located at the current cursor.</returns>
    private Token Peek() => _tokens[_current];

    /// <summary>
    /// Peek at the previous token from the cursor.
    /// </summary>
    /// <returns>The token located one position behind the cursor.</returns>
    private Token Previous() => _tokens[_current - 1];
}
