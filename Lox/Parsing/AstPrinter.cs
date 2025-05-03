namespace Lox.Parsing;

public class AstPrinter : IExprVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(BinaryExpr expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);

    public string VisitGroupingExpr(GroupingExpr expr)
        => Parenthesize("group", expr.Expression);

    public string VisitLiteralExpr(LiteralExpr expr)
        => expr.Value?.ToString() ?? "nil";

    public string VisitUnaryExpr(UnaryExpr expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Right);

    public string VisitVariableExpr(VariableExpr expr)
        => $"(variable {expr.Name.Lexeme})";

    public string VisitAssignExpr(AssignExpr expr)
        => Parenthesize($"assign {expr.Name.Lexeme}", expr.Value);

    public string VisitLogicalExpr(LogicalExpr expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);

    internal string Parenthesize(string name, params IEnumerable<Expr> expressions)
        => $"({name} {string.Join(' ', expressions.Select(expr => expr.Accept(this)))})";
}
