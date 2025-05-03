using Lox.Tokens;

namespace Lox.Parsing;

public interface IStmtVisitor
{
    void VisitExpressionStmt(ExpressionStmt stmt);
    void VisitPrintStmt(PrintStmt stmt);
    void VisitVarStmt(VarStmt stmt);
}

public abstract record Stmt
{
    public abstract void Accept(IStmtVisitor visitor);
}

public record ExpressionStmt(Expr Expression) : Stmt
{
    public override void Accept(IStmtVisitor visitor) => visitor.VisitExpressionStmt(this);
}

public record PrintStmt(Expr Expression) : Stmt
{
    public override void Accept(IStmtVisitor visitor) => visitor.VisitPrintStmt(this);
}

public record VarStmt(Token Name, Expr? Initializer) : Stmt
{
    public override void Accept(IStmtVisitor visitor) => visitor.VisitVarStmt(this);
}
