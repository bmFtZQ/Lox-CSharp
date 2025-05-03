using Lox.Tokens;

namespace Lox.Parsing;

public abstract record Expr
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record Binary(Expr Left, Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinary(this);
}

public record Grouping(Expr Expression) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGrouping(this);
}

public record Literal(object? Value) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteral(this);
}

public record Unary(Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnary(this);
}
