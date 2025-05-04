using Lox.Interpreting;
using Lox.Parsing;
using Lox.Tokens;

namespace Lox.Analysis;

/// <summary>
/// Perform code analysis and resolve variable definitions and usages.
/// </summary>
/// <param name="interpreter">The interpreter to resolve with.</param>
public class Resolver(Interpreter interpreter) : IStmtVisitor, IExprVisitor<object?>
{
    private readonly List<Dictionary<string, bool>> _scopes = [];
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;

    private enum FunctionType
    {
        None,
        Function,
        Initializer,
        Method
    }

    private enum ClassType
    {
        None,
        Class,
        SubClass
    }

    /// <summary>
    /// Analyze and resolve a series of statements.
    /// </summary>
    /// <param name="statements">The statements to analyze.</param>
    public void Resolve(IReadOnlyList<Stmt?> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    /// <summary>
    /// Analyze and resolve a single statement.
    /// </summary>
    /// <param name="statement">The statement to analyze.</param>
    private void Resolve(Stmt? statement) => statement?.Accept(this);

    /// <summary>
    /// Analyze and resolve a single expression.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    private void Resolve(Expr? expression) => expression?.Accept(this);

    /// <summary>
    /// Create a new variable block scope.
    /// </summary>
    private void BeginScope() => _scopes.Add([]);

    /// <summary>
    /// Exit a block scope.
    /// </summary>
    private void EndScope() => _scopes.RemoveAt(_scopes.Count - 1);

    /// <summary>
    /// Declare that a variable exists.
    /// </summary>
    /// <param name="name">The variable to declare.</param>
    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;

        var scope = _scopes[^1];
        scope[name.Lexeme] = false;
    }

    /// <summary>
    /// Resolve a variable reference, linking it to its definition.
    /// </summary>
    /// <param name="expr">The expression containing the reference.</param>
    /// <param name="name">The name of the variable.</param>
    private void ResolveLocal(Expr expr, Token name)
    {
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, _scopes.Count - 1 - i);
            }
        }
    }

    /// <summary>
    /// Resolve a variable references for a function statement.
    /// </summary>
    /// <param name="function">The function to resolve references for.</param>
    /// <param name="type">The type of function to resolve references for.</param>
    private void ResolveFunction(FunctionStmt function, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope();

        foreach (var param in function.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    /// <summary>
    /// Resolve a variable references for a function expression.
    /// </summary>
    /// <param name="function">The function to resolve references for.</param>
    /// <param name="type">The type of function to resolve references for.</param>
    private void ResolveFunction(FunctionExpr function, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope();

        foreach (var param in function.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    /// <summary>
    /// Define that a variable exists and has been defined.
    /// </summary>
    /// <param name="name"></param>
    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;

        _scopes[^1][name.Lexeme] = true;
    }

    public void VisitExpressionStmt(ExpressionStmt stmt)
    {
        Resolve(stmt.Expression);
    }

    public void VisitPrintStmt(PrintStmt stmt)
    {
        Resolve(stmt.Expression);
    }

    public void VisitReturnStmt(ReturnStmt stmt)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (_currentFunction)
        {
            case FunctionType.None:
                Program.Error(stmt.Keyword, "Can't return from top-level code.");
                break;

            case FunctionType.Initializer when stmt.Value is not null:
                Program.Error(stmt.Keyword, "Can't return a value from an initializer.");
                break;
        }

        Resolve(stmt.Value);
    }

    public void VisitVarStmt(VarStmt stmt)
    {
        Declare(stmt.Name);

        if (stmt.Initializer is not null)
        {
            Resolve(stmt.Initializer);
        }

        Define(stmt.Name);
    }

    public void VisitFunctionStmt(FunctionStmt stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.Function);
    }

    public void VisitClassStmt(ClassStmt stmt)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;

        Declare(stmt.Name);
        Define(stmt.Name);

        if (stmt.SuperClass is not null)
        {
            _currentClass = ClassType.SubClass;
            if (stmt.Name.Lexeme == stmt.SuperClass.Name.Lexeme)
            {
                Program.Error(stmt.SuperClass.Name, "A class can't inherit from itself.");
            }

            Resolve(stmt.SuperClass);

            BeginScope();
            _scopes[^1]["super"] = true;
        }

        BeginScope();
        _scopes[^1]["this"] = true;

        foreach (var method in stmt.Methods)
        {
            var declaration = method.Name.Lexeme == "init"
                ? FunctionType.Initializer
                : FunctionType.Method;
            ResolveFunction(method, declaration);
        }

        EndScope();
        _currentClass = enclosingClass;

        if (stmt.SuperClass is not null) EndScope();
    }

    public void VisitBlockStmt(BlockStmt stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
    }

    public void VisitIfStmt(IfStmt stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        Resolve(stmt.ElseBranch);
    }

    public void VisitWhileStmt(WhileStmt stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
    }

    public object? VisitBinaryExpr(BinaryExpr expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitGroupingExpr(GroupingExpr expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(LiteralExpr expr)
    {
        return null;
    }

    public object? VisitUnaryExpr(UnaryExpr expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariableExpr(VariableExpr expr)
    {
        if (_scopes.Count > 0 && _scopes[^1].ContainsKey(expr.Name.Lexeme) && _scopes[^1][expr.Name.Lexeme] == false)
        {
            Program.Error(expr.Name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitAssignExpr(AssignExpr expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitLogicalExpr(LogicalExpr expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCallExpr(CallExpr expr)
    {
        Resolve(expr.Callee);
        foreach (var arg in expr.Arguments)
        {
            Resolve(arg);
        }

        return null;
    }

    public object? VisitGetExpr(GetExpr expr)
    {
        Resolve(expr.Object);
        return null;
    }

    public object? VisitSetExpr(SetExpr expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    public object? VisitThisExpr(ThisExpr expr)
    {
        if (_currentClass == ClassType.None)
        {
            Program.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitSuperExpr(SuperExpr expr)
    {
        if (_currentClass == ClassType.None)
        {
            Program.Error(expr.Keyword, "Can't use 'super' outside of a class.");
        }
        else if (_currentClass != ClassType.SubClass)
        {
            Program.Error(expr.Keyword, "Can't use 'super' in a class with no superclass.");
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitFunctionExpr(FunctionExpr expr)
    {
        ResolveFunction(expr, FunctionType.Function);
        return null;
    }
}
