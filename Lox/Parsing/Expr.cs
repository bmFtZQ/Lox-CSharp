using Lox.Tokens;

namespace Lox.Parsing;

public interface IExprVisitor<out T>
{
    T VisitBinary(BinaryExpr expr);
    T VisitGrouping(GroupingExpr expr);
    T VisitLiteral(LiteralExpr expr);
    T VisitUnary(UnaryExpr expr);
}

public abstract record Expr
{
    public abstract T Accept<T>(IExprVisitor<T> visitor);
}

public record BinaryExpr(Expr Left, Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitBinary(this);
}

public record GroupingExpr(Expr Expression) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitGrouping(this);
}

public record LiteralExpr(object? Value) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitLiteral(this);
}

public record UnaryExpr(Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitUnary(this);
}
