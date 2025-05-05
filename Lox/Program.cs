using Lox.Analysis;
using Lox.Interpreting;
using Lox.Parsing;
using Lox.Tokens;

namespace Lox;

internal static class Program
{
    internal static bool HadError = false;
    internal static bool HadRuntimeError = false;

    internal static void Main(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: Lox <file>");
                System.Environment.Exit(64);
                break;

            case 1:
                RunFile(args[0]);
                break;

            default:
                RunPrompt();
                break;
        }
    }

    internal static void RunFile(string filename)
    {
        var source = File.ReadAllText(filename);
        Run(source);

        if (HadError) System.Environment.Exit(65);
        if (HadRuntimeError) System.Environment.Exit(70);
    }

    internal static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null) break;
            Run(line);
            HadError = false;
        }
    }

    internal static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens);
        var statements = parser.Parse();

        // Return on any parsing errors.
        if (HadError) return;

        var interpreter = new Interpreter();
        var resolver = new Resolver(interpreter);
        resolver.Resolve(statements);

        // Return on any resolution errors.
        if (HadError) return;
        interpreter.Interpret(statements);
    }

    internal static void Error(int line, string message) =>
        Report(line, "", message);

    internal static void Error(Token token, string message)
    {
        Report(token.Line, token.Type == TokenType.Eof
            ? " at end"
            : $" at '{token.Lexeme}'", message);
    }

    internal static void RunTimeError(RunTimeException exception)
    {
        Console.Error.WriteLine($"{exception.Message}\n[line {exception.Token?.Line}]");
        HadRuntimeError = true;
    }

    internal static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}
