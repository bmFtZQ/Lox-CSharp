using Lox.Parsing;
using Lox.Tokens;

namespace Lox.Interpreting;

public class RunTimeException(Token token, string message) : Exception(message)
{
    public Token Token { get; } = token;
}

public class Interpreter : IExprVisitor<object?>, IStmtVisitor
{
    private readonly Environment _environment = new();

    /// <summary>
    /// Interpret a list of statements.
    /// </summary>
    /// <param name="statements">The statements to interpret.</param>
    public void Interpret(IEnumerable<Stmt?> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RunTimeException exception)
        {
            Program.RunTimeError(exception);
        }
    }

    /// <summary>
    /// Evaluate a binary operator expression.
    /// </summary>
    /// <param name="expr">The binary expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    /// <exception cref="RunTimeException">
    /// Thrown if operand types are incorrect for the specified operator.
    /// </exception>
    public object? VisitBinary(BinaryExpr expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! - (double)right!;

            case TokenType.Slash:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! / (double)right!;

            case TokenType.Star:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! * (double)right!;

            // Concatenate strings when left operand is string.
            case TokenType.Plus when left is string l:
                return l + Stringify(right);

            // Concatenate strings when right operand is string.
            case TokenType.Plus when right is string r:
                return Stringify(left) + r;

            case TokenType.Plus when left is double l && right is double r:
                return l + r;

            case TokenType.Plus:
                throw new RunTimeException(expr.Operator, "Operands must be two numbers or at least one string.");

            case TokenType.Greater:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! > (double)right!;

            case TokenType.GreaterEqual:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! >= (double)right!;

            case TokenType.Less:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! < (double)right!;

            case TokenType.LessEqual:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left! <= (double)right!;

            case TokenType.EqualEqual:
                CheckNumberOperand(expr.Operator, left, right);
                return left == right;

            case TokenType.BangEqual:
                CheckNumberOperand(expr.Operator, left, right);
                return left != right;

            default:
                return null;
        }
    }

    /// <summary>
    /// Evaluate a grouping expression.
    /// </summary>
    /// <param name="expr">The grouping expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    public object? VisitGrouping(GroupingExpr expr)
    {
        return Evaluate(expr.Expression);
    }

    /// <summary>
    /// Evaluate a literal expression.
    /// </summary>
    /// <param name="expr">The literal expression to evaluate.</param>
    /// <returns>The literal value from the expression.</returns>
    public object? VisitLiteral(LiteralExpr expr)
    {
        return expr.Value;
    }

    /// <summary>
    /// Evaluate a unary expression.
    /// </summary>
    /// <param name="expr">The unary expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    public object? VisitUnary(UnaryExpr expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right!;

            case TokenType.Bang:
                return !IsTruthy(right);

            default:
                return null;
        }
    }

    /// <summary>
    /// Evaluate a variable expression.
    /// </summary>
    /// <param name="expr">The variable expression to evaluate.</param>
    /// <returns>The value from the variable.</returns>
    public object? VisitVariable(VariableExpr expr) => _environment.Get(expr.Name);

    /// <summary>
    /// Evaluate an expression.
    /// </summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt? statement)
    {
        statement?.Accept(this);
    }

    /// <summary>
    /// Verify that all operands are numbers.
    /// </summary>
    /// <param name="token">The operator token to report the error with.</param>
    /// <param name="operands">The operands to verify.</param>
    /// <exception cref="RunTimeException">
    /// Thrown if any operand is not a number.
    /// </exception>
    private static void CheckNumberOperand(Token token, params object?[] operands)
    {
        if (operands.Any(o => o is not double))
        {
            throw new RunTimeException(token,
                operands.Length == 1 ? "Operand must be a number." : "Operands must be numbers.");
        }
    }

    /// <summary>
    /// Test whether a value is truthy or not.
    /// </summary>
    /// <param name="obj">The object to test.</param>
    /// <returns>True if the object is truthy, false otherwise.</returns>
    private static bool IsTruthy(object? obj) => obj switch
    {
        null => false,
        bool b => b,
        _ => true
    };

    /// <summary>
    /// Convert a value to a string.
    /// </summary>
    /// <param name="value">The value to convert to a string.</param>
    /// <returns>A string representing the specified value.</returns>
    public static string Stringify(object? value) => value switch
    {
        bool b => b ? "true" : "false",
        not null => value.ToString(),
        _ => null
    } ?? "nil";

    /// <summary>
    /// Execute an expression statement, evaluating the expression and
    /// discarding value.
    /// </summary>
    /// <param name="stmt">The statement to evaluate.</param>
    public void VisitExpressionStmt(ExpressionStmt stmt)
    {
        Evaluate(stmt.Expression);
    }

    /// <summary>
    /// Execute a print statement, evaluating the expression and printing output
    /// to console.
    /// </summary>
    /// <param name="stmt">The statement to evaluate.</param>
    public void VisitPrintStmt(PrintStmt stmt)
    {
        var value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
    }

    /// <summary>
    /// Execute a variable declaration statement, evaluating the optional
    /// initializer expression.
    /// </summary>
    /// <param name="stmt">The statement to evaluate.</param>
    public void VisitVarStmt(VarStmt stmt)
    {
        var value = stmt.Initializer is not null
            ? Evaluate(stmt.Initializer)
            : null;

        _environment.Define(stmt.Name.Lexeme, value);
    }
}
