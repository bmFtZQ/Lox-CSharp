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
    public List<Stmt?> Parse()
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
            if (Match(TokenType.Var)) return VarDeclaration();
            if (Match(TokenType.Class)) return ClassDeclaration();
            if (Match(TokenType.Fun))
            {
                // Handle case of function expression appearing in expression
                // statement.
                if (Peek().Type == TokenType.LeftParenthesis)
                {
                    _current--;
                    return ExpressionStatement();
                }

                return Function();
            }

            return Statement();
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
        var name = Consume(TokenType.Identifier, "Expect variable name.");

        var initializer = Match(TokenType.Equal)
            ? Expression()
            : null;

        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarStmt(name, initializer);
    }

    /// <summary>
    /// Scan for class declaration statement.
    /// </summary>
    /// <returns>An AST populated with the parse class.</returns>
    private ClassStmt ClassDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect class name.");

        var superclass = Match(TokenType.Less)
            ? new VariableExpr(Consume(TokenType.Identifier, "Expect superclass name."))
            : null;

        Consume(TokenType.LeftBrace, "Expect '{' before class body.");

        List<FunctionStmt> methods = [];
        List<FunctionStmt> staticMethods = [];
        while (!Check(TokenType.RightBrace) && !IsAtEnd)
        {
            if (Match(TokenType.Class))
            {
                var func = Function("static method");
                if (func.Parameters.Count > 0 && func.Name.Lexeme == "init")
                {
                    Error(func.Name, "Static method init cannot have parameters.");
                }

                staticMethods.Add(func);
            }
            else
            {
                methods.Add(Function("method"));
            }
        }

        Consume(TokenType.RightBrace, "Expect '}' after class body.");

        return new ClassStmt(name, superclass, methods, staticMethods);
    }

    /// <summary>
    /// Scan for function or method declaration.
    /// </summary>
    /// <param name="kind">Used for changing the displayed errors.</param>
    /// <returns>An AST with the parsed function declaration.</returns>
    private FunctionStmt Function(string kind = "function")
    {
        var name = Consume(TokenType.Identifier, $"Expect {kind} name.");

        Consume(TokenType.LeftParenthesis, $"Expect '(' after {kind} name.");

        var parameters = FunctionParameters();

        Consume(TokenType.RightParenthesis, $"Expect ')' after parameters.");
        Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
        var body = Block();

        return new FunctionStmt(name, parameters, body);
    }

    /// <summary>
    /// Parse a function's parameter list.
    /// </summary>
    /// <returns>
    /// A list of Tokens representing a function's parameters.
    /// </returns>
    private List<Token> FunctionParameters()
    {
        List<Token> parameters = [];
        if (!Check(TokenType.RightParenthesis))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(
                    Consume(TokenType.Identifier, "Expect parameter name."));
            } while (Match(TokenType.Comma));
        }

        return parameters;
    }

    /// <summary>
    /// Scan for statements, will cascade to lower level statements.
    /// </summary>
    /// <returns>An AST populate with the parsed statement.</returns>
    private Stmt? Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.LeftBrace)) return new BlockStmt(Block());

        return !Match(TokenType.Semicolon)
            ? ExpressionStatement()
            : null;
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
    /// Scan for return statement.
    /// </summary>
    /// <returns>An AST populated with the parsed statement.</returns>
    private ReturnStmt ReturnStatement()
    {
        var keyword = Previous();

        var value = !Check(TokenType.Semicolon)
            ? Expression()
            : null;

        Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new ReturnStmt(keyword, value);
    }

    /// <summary>
    /// Scan for expression statement.
    /// </summary>
    /// <returns>An AST populated with the parsed statement.</returns>
    private ExpressionStmt ExpressionStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new ExpressionStmt(value);
    }

    /// <summary>
    /// Scan for block statement.
    /// </summary>
    /// <returns>An AST populated with the parsed inner statements.</returns>
    private List<Stmt?> Block()
    {
        List<Stmt?> statements = [];

        while (!Check(TokenType.RightBrace) && !IsAtEnd)
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }

    /// <summary>
    /// Scan for if statement.
    /// </summary>
    /// <returns>An AST populate with the parsed if statement.</returns>
    private IfStmt IfStatement()
    {
        Consume(TokenType.LeftParenthesis, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParenthesis, "Expect ')' after if condition.");

        var thenBranch = Statement();

        var elseBranch = Match(TokenType.Else)
            ? Statement()
            : null;

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    /// <summary>
    /// Scan for while loop statement.
    /// </summary>
    /// <returns>An AST populated with the parsed while statement.</returns>
    private WhileStmt WhileStatement()
    {
        Consume(TokenType.LeftParenthesis, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParenthesis, "Expect ')' after if condition.");

        var body = Statement();

        return new WhileStmt(condition, body);
    }

    /// <summary>
    /// Scan for for-loop statement, then convert to while loop.
    /// </summary>
    /// <returns>
    /// An AST populated with the parsed and converted for-loop.
    /// </returns>
    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParenthesis, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else if (Match(TokenType.Var))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        var condition = !Check(TokenType.Semicolon)
            ? Expression()
            : new LiteralExpr(true);

        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

        var increment = !Check(TokenType.RightParenthesis)
            ? Expression()
            : null;

        Consume(TokenType.RightParenthesis, "Expect ')' after for clauses.");

        var body = Statement();

        if (increment is not null)
        {
            body = new BlockStmt([body, new ExpressionStmt(increment)]);
        }

        // Create while-loop that can 
        Stmt whileLoop = new WhileStmt(condition, body);

        // Introduce new scope for initialized variable if present.
        var loopStmt = initializer is not null
            ? new BlockStmt([initializer, whileLoop])
            : whileLoop;

        return loopStmt;
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
        var expr = Or();

        if (Match(TokenType.Equal,
                TokenType.PlusEqual,
                TokenType.MinusEqual,
                TokenType.StarEqual,
                TokenType.SlashEqual))
        {
            var op = Previous();

            // Search for additional assignment expressions or navigate further
            // down the grammar.
            var value = Assignment();

            // Check for augmented assignment operator (i += 1 to i = i + 1)
            var token = op.Type switch
            {
                TokenType.PlusEqual => new Token(TokenType.Plus, "+", null, op.Line),
                TokenType.MinusEqual => new Token(TokenType.Minus, "-", null, op.Line),
                TokenType.StarEqual => new Token(TokenType.Star, "*", null, op.Line),
                TokenType.SlashEqual => new Token(TokenType.Slash, "/", null, op.Line),
                _ => null
            };

            if (token is not null)
            {
                value = new BinaryExpr(expr, token, value);
            }

            // Search for 'l-value' that can be assigned to.
            switch (expr)
            {
                case VariableExpr varExpr:
                    return new AssignExpr(varExpr.Name, value);

                case GetExpr getExpr:
                    return new SetExpr(getExpr.Object, getExpr.Index, value, getExpr.Token);

                default:
                    Error(op, "Invalid assignment target.");
                    break;
            }
        }

        return expr;
    }

    /// <summary>
    /// Scan for logical or expression.
    /// </summary>
    /// <returns>An AST populated with the parsed expression.</returns>
    private Expr Or()
    {
        var expr = And();

        while (Match(TokenType.Or))
        {
            var op = Previous();
            var right = And();
            expr = new LogicalExpr(op, expr, right);
        }

        return expr;
    }

    /// <summary>
    /// Scan for logical and expression.
    /// </summary>
    /// <returns>An AST populated with the parsed expression.</returns>
    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var op = Previous();
            var right = And();
            expr = new LogicalExpr(op, expr, right);
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

        return FunctionExpression();
    }

    /// <summary>
    /// Scan for function expression, navigating further down hierarchy if no
    /// function expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr FunctionExpression()
    {
        if (Match(TokenType.Fun))
        {
            var keyword = Previous();
            Consume(TokenType.LeftParenthesis, "Expect '(' after 'fun'.");

            var parameters = FunctionParameters();

            Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expect '{' before function body.");
            var body = Block();

            return new FunctionExpr(keyword, parameters, body);
        }

        return IfExpression();
    }

    private Expr IfExpression()
    {
        if (Match(TokenType.If))
        {
            var token = Previous();
            Consume(TokenType.LeftParenthesis, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RightParenthesis, "Expect ')' after if condition.");
            var thenExpr = Expression();
            Consume(TokenType.Else, "Expect 'else' after expression.");
            var elseExpr = Expression();
            return new IfExpr(token, condition, thenExpr, elseExpr);
        }

        return Call();
    }

    /// <summary>
    /// Scan for call expression,navigating further down hierarchy if no call
    /// expression found.
    /// </summary>
    /// <returns>An AST populated with the parsed expressions.</returns>
    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParenthesis))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expect property name after '.'.");
                expr = new GetExpr(expr, new LiteralExpr(name.Lexeme), name);
            }
            else if (Match(TokenType.LeftBracket))
            {
                var token = Peek();
                var propExpr = Expression();
                Consume(TokenType.RightBracket, "Expect ']' after property expression.");
                expr = new GetExpr(expr, propExpr, token);
            }
            else
            {
                break;
            }
        }

        return expr;
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

        if (Match(TokenType.LeftBracket))
        {
            var token = Previous();
            List<Expr> expressions = [];

            if (!Check(TokenType.RightBracket))
            {
                do
                {
                    expressions.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightBracket, "Expect ']' after array elements.");

            return new ArrayExpr([..expressions], token);
        }

        if (Match(TokenType.This)) return new ThisExpr(Previous());
        if (Match(TokenType.Super))
        {
            var keyword = Previous();
            Consume(TokenType.Dot, "Expect '.' after 'super'.");
            var method = Consume(TokenType.Identifier, "Expect superclass method name.");
            return new SuperExpr(keyword, method);
        }

        if (Match(TokenType.Identifier))
        {
            return new VariableExpr(Previous());
        }

        throw Error(Peek(), "Expect expression.");
    }

    /// <summary>
    /// Parse a call expression argument list and construct CallExpr.
    /// </summary>
    /// <param name="callee">The expression that evaluates the callee.</param>
    /// <returns>An AST populated with the completed call expression.</returns>
    private CallExpr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];
        if (!Check(TokenType.RightParenthesis))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }

                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        var paren = Consume(TokenType.RightParenthesis, "Expect ')' after arguments.");

        return new CallExpr(callee, paren, arguments);
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
