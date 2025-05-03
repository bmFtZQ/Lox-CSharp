namespace Lox.Parsing;

public class AstPrinter : IExprVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitBinary(BinaryExpr expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);

    public string VisitGrouping(GroupingExpr expr)
        => Parenthesize("group", expr.Expression);

    public string VisitLiteral(LiteralExpr expr)
        => expr.Value?.ToString() ?? "nil";

    public string VisitUnary(UnaryExpr expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Right);

    internal string Parenthesize(string name, params IEnumerable<Expr> expressions)
        => $"({name} {string.Join(' ', expressions.Select(expr => expr.Accept(this)))})";
}
