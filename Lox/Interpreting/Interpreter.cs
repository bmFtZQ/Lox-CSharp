using Lox.Interpreting.NativeCallables;
using Lox.Parsing;
using Lox.Tokens;

namespace Lox.Interpreting;

public class RunTimeException(Token? token = null, string? message = null) : Exception(message)
{
    public Token? Token { get; } = token;
}

/// <summary>
/// Exception that holds a value, used to unwind stack from function call.
/// </summary>
/// <param name="value">The value to return.</param>
public class ReturnException(object? value = null) : RunTimeException
{
    public object? Value { get; } = value;
}

public class Interpreter : IExprVisitor<object?>, IStmtVisitor
{
    private Environment _environment;
    public Environment Globals { get; } = new();
    private readonly Dictionary<Expr, int> _locals = [];

    public Interpreter()
    {
        Globals.Define("clock", ClockCallable.Instance);
        _environment = Globals;
    }

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
    /// Assign a depth for a variable, so it can be tracked to its original
    /// environment.
    /// </summary>
    /// <param name="expr">The variable expression to resolve.</param>
    /// <param name="depth">How many levels further up the variable is defined.</param>
    public void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }

    /// <summary>
    /// Evaluate a binary operator expression.
    /// </summary>
    /// <param name="expr">The binary expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    /// <exception cref="RunTimeException">
    /// Thrown if operand types are incorrect for the specified operator.
    /// </exception>
    public object? VisitBinaryExpr(BinaryExpr expr)
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
                return Equals(left, right);

            case TokenType.BangEqual:
                return !Equals(left, right);

            default:
                return null;
        }
    }

    /// <summary>
    /// Evaluate a grouping expression.
    /// </summary>
    /// <param name="expr">The grouping expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    public object? VisitGroupingExpr(GroupingExpr expr)
    {
        return Evaluate(expr.Expression);
    }

    /// <summary>
    /// Evaluate a literal expression.
    /// </summary>
    /// <param name="expr">The literal expression to evaluate.</param>
    /// <returns>The literal value from the expression.</returns>
    public object? VisitLiteralExpr(LiteralExpr expr)
    {
        return expr.Value;
    }

    /// <summary>
    /// Evaluate a unary expression.
    /// </summary>
    /// <param name="expr">The unary expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    public object? VisitUnaryExpr(UnaryExpr expr)
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
    public object? VisitVariableExpr(VariableExpr expr) => LookUpVariable(expr.Name, expr);

    /// <summary>
    /// Evaluate an assignment expression.
    /// </summary>
    /// <param name="expr">The assignment expression to evaluate.</param>
    /// <returns>The value that was assigned.</returns>
    public object? VisitAssignExpr(AssignExpr expr)
    {
        var value = Evaluate(expr.Value);

        if (_locals.TryGetValue(expr, out var distance))
        {
            _environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            Globals.Assign(expr.Name, value);
        }

        return value;
    }

    /// <summary>
    /// Evaluate a logical expression, short-circuiting the evaluation.
    /// </summary>
    /// <param name="expr">The logical expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    public object? VisitLogicalExpr(LogicalExpr expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    /// <summary>
    /// Evaluate a call expression, executing the callable and returning value.
    /// </summary>
    /// <param name="expr">The call expression to evaluate.</param>
    /// <returns>The value computed from the function call.</returns>
    /// <exception cref="RunTimeException">Thrown if the number of arguments
    /// does not match the callee's number of parameters.</exception>
    public object? VisitCallExpr(CallExpr expr)
    {
        var callee = Evaluate(expr.Callee);

        var arguments = expr.Arguments.Select(Evaluate).ToArray();

        var function = callee as ILoxCallable ?? throw new RunTimeException(
            expr.Parenthesis, "Can only call functions and classes.");

        if (arguments.Length != function.Arity)
        {
            throw new RunTimeException(expr.Parenthesis,
                $"Expected {function.Arity} arguments but got {arguments.Length}.");
        }

        return function.Call(this, arguments);
    }

    /// <summary>
    /// Evaluate a function expression, creating a new anonymous function.
    /// </summary>
    /// <param name="expr">The function expression to evaluate.</param>
    /// <returns>The new anonymous function.</returns>
    public object VisitFunctionExpr(FunctionExpr expr)
    {
        return new LoxFunction(expr.Parameters, expr.Body, _environment);
    }

    /// <summary>
    /// Look up a variable and retrieve its value.
    /// </summary>
    /// <param name="name">The variable to look up.</param>
    /// <param name="expr">The expression where the lookup occurs.</param>
    /// <returns>The local variable, global variable, or null.</returns>
    private object? LookUpVariable(Token name, Expr expr)
    {
        return _locals.TryGetValue(expr, out var distance)
            ? _environment.GetAt(distance, name.Lexeme)
            : Globals.Get(name);
    }

    /// <summary>
    /// Evaluate an expression.
    /// </summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <returns>The value computed from the expression.</returns>
    private object? Evaluate(Expr? expr)
    {
        return expr?.Accept(this);
    }

    /// <summary>
    /// Execute a statement.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    private void Execute(Stmt? statement)
    {
        statement?.Accept(this);
    }

    /// <summary>
    /// Execute a series of statements, using the specified environment.
    /// </summary>
    /// <param name="statements">The statements to execute.</param>
    /// <param name="env">The environment to use.</param>
    public void ExecuteBlock(IEnumerable<Stmt?> statements, Environment env)
    {
        var previous = _environment;

        try
        {
            _environment = env;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
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
    /// <param name="stmt">The statement to execute.</param>
    public void VisitExpressionStmt(ExpressionStmt stmt)
    {
        Evaluate(stmt.Expression);
    }

    /// <summary>
    /// Execute a print statement, evaluating the expression and printing output
    /// to console.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    public void VisitPrintStmt(PrintStmt stmt)
    {
        var value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
    }

    /// <summary>
    /// Execute a return statement, evaluating the expression and returning.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <exception cref="ReturnException">Thrown to return the value, must be
    /// caught by the called function.</exception>
    public void VisitReturnStmt(ReturnStmt stmt)
    {
        var value = stmt.Value is not null
            ? Evaluate(stmt.Value)
            : null;

        throw new ReturnException(value);
    }

    /// <summary>
    /// Execute a variable declaration statement, evaluating the optional
    /// initializer expression.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    public void VisitVarStmt(VarStmt stmt)
    {
        var value = stmt.Initializer is not null
            ? Evaluate(stmt.Initializer)
            : null;

        _environment.Define(stmt.Name.Lexeme, value);
    }

    /// <summary>
    /// Execute a function declaration statement.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    public void VisitFunctionStmt(FunctionStmt stmt)
    {
        var function = new LoxFunction(stmt.Parameters, stmt.Body, _environment, stmt.Name.Lexeme);
        _environment.Define(stmt.Name.Lexeme, function);
    }

    /// <summary>
    /// Execute a block statement, executing each of the inner statements.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    public void VisitBlockStmt(BlockStmt stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
    }

    /// <summary>
    /// Execute an if statement.
    /// </summary>
    /// <param name="stmt">The if statement to execute.</param>
    public void VisitIfStmt(IfStmt stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch is not null)
        {
            Execute(stmt.ElseBranch);
        }
    }

    /// <summary>
    /// Execute a while loop statement.
    /// </summary>
    /// <param name="stmt">The while statement to execute.</param>
    public void VisitWhileStmt(WhileStmt stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }
    }
}
