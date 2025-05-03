namespace Lox.Parsing;

public interface IVisitor<out T>
{
    T VisitBinary(Binary expr);
    T VisitGrouping(Grouping expr);
    T VisitLiteral(Literal expr);
    T VisitUnary(Unary expr);
}
