namespace Lox;

internal static class Program
{
    internal static bool HadError = false;

    internal static void Main(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: Lox <file>");
                Environment.Exit(64);
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

        if (HadError) Environment.Exit(65);
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

        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    internal static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}
