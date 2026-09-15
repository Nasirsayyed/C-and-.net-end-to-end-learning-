using ConsoleCalculator;

var calculator = new Calculator();
var history = new List<string>();

Console.WriteLine("Console Calculator — supports + - * / and parentheses.");
Console.WriteLine("Type 'history' to see past calculations, or 'exit' to quit.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (input.Equals("history", StringComparison.OrdinalIgnoreCase))
    {
        if (history.Count == 0)
        {
            Console.WriteLine("(no calculations yet)");
        }
        else
        {
            foreach (var entry in history)
            {
                Console.WriteLine(entry);
            }
        }
        continue;
    }

    try
    {
        var result = calculator.Evaluate(input);
        var entry = $"{input} = {result}";
        history.Add(entry);
        Console.WriteLine(result);
    }
    catch (Exception ex) when (ex is FormatException or DivideByZeroException or ArgumentException)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
