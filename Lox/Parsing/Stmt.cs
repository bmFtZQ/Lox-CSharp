using Lox.Tokens;

namespace Lox.Parsing;

public interface IStmtVisitor
{
    void VisitExpressionStmt(ExpressionStmt stmt);
    void VisitPrintStmt(PrintStmt stmt);
    void VisitVarStmt(VarStmt stmt);
    void VisitBlockStmt(BlockStmt stmt);
    void VisitIfStmt(IfStmt stmt);
    void VisitWhileStmt(WhileStmt stmt);
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

public record BlockStmt(IEnumerable<Stmt?> Statements) : Stmt
{
    public override void Accept(IStmtVisitor visitor) => visitor.VisitBlockStmt(this);
}

public record IfStmt(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt
{
    public override void Accept(IStmtVisitor visitor) => visitor.VisitIfStmt(this);
}

public record WhileStmt(Expr Condition, Stmt Body) : Stmt
{
    public override void Accept(IStmtVisitor visitor) => visitor.VisitWhileStmt(this);
}
