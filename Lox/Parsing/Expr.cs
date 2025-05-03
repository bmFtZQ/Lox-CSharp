using Lox.Tokens;

namespace Lox.Parsing;

public interface IExprVisitor<out T>
{
    T VisitBinaryExpr(BinaryExpr expr);
    T VisitGroupingExpr(GroupingExpr expr);
    T VisitLiteralExpr(LiteralExpr expr);
    T VisitUnaryExpr(UnaryExpr expr);
    T VisitVariableExpr(VariableExpr expr);
    T VisitAssignExpr(AssignExpr expr);
    T VisitLogicalExpr(LogicalExpr expr);
}

public abstract record Expr
{
    public abstract T Accept<T>(IExprVisitor<T> visitor);
}

public record BinaryExpr(Expr Left, Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitBinaryExpr(this);
}

public record GroupingExpr(Expr Expression) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitGroupingExpr(this);
}

public record LiteralExpr(object? Value) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitLiteralExpr(this);
}

public record UnaryExpr(Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitUnaryExpr(this);
}

public record VariableExpr(Token Name) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitVariableExpr(this);
}

public record AssignExpr(Token Name, Expr Value) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitAssignExpr(this);
}

public record LogicalExpr(Token Operator, Expr Left, Expr Right) : Expr
{
    public override T Accept<T>(IExprVisitor<T> visitor) => visitor.VisitLogicalExpr(this);
}
