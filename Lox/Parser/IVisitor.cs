namespace Lox.Parser;

public interface IVisitor<T>
{
    T VisitBinary(Binary expr);
    T VisitGrouping(Grouping expr);
    T VisitLiteral(Literal expr);
    T VisitUnary(Unary expr);
}
