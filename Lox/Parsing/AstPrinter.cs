namespace Lox.Parsing;

public class AstPrinter : IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitBinary(Binary expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);

    public string VisitGrouping(Grouping expr)
        => Parenthesize("group", expr.Expression);

    public string VisitLiteral(Literal expr)
        => expr.Value?.ToString() ?? "nil";

    public string VisitUnary(Unary expr)
        => Parenthesize(expr.Operator.Lexeme, expr.Right);

    internal string Parenthesize(string name, params IEnumerable<Expr> expressions)
        => $"({name} {string.Join(' ', expressions.Select(expr => expr.Accept(this)))})";
}
