namespace ConsoleCalculator;

/// <summary>
/// A recursive-descent expression evaluator supporting + - * / and parentheses,
/// following standard operator precedence.
/// Grammar:
///   expression := term (('+' | '-') term)*
///   term       := factor (('*' | '/') factor)*
///   factor     := number | '(' expression ')' | '-' factor
/// </summary>
public class Calculator
{
    public double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression cannot be empty.", nameof(expression));
        }

        var tokens = Tokenize(expression);
        var position = 0;
        var result = ParseExpression(tokens, ref position);

        if (position != tokens.Count)
        {
            throw new FormatException($"Unexpected token '{tokens[position]}' in expression.");
        }

        return result;
    }

    private static List<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        var i = 0;

        while (i < expression.Length)
        {
            var c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if ("+-*/()".IndexOf(c) >= 0)
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                var start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    i++;
                }
                tokens.Add(expression[start..i]);
                continue;
            }

            throw new FormatException($"Unexpected character '{c}' in expression.");
        }

        return tokens;
    }

    private static double ParseExpression(List<string> tokens, ref int position)
    {
        var result = ParseTerm(tokens, ref position);

        while (position < tokens.Count && (tokens[position] == "+" || tokens[position] == "-"))
        {
            var op = tokens[position++];
            var rhs = ParseTerm(tokens, ref position);
            result = op == "+" ? result + rhs : result - rhs;
        }

        return result;
    }

    private static double ParseTerm(List<string> tokens, ref int position)
    {
        var result = ParseFactor(tokens, ref position);

        while (position < tokens.Count && (tokens[position] == "*" || tokens[position] == "/"))
        {
            var op = tokens[position++];
            var rhs = ParseFactor(tokens, ref position);

            if (op == "/" && rhs == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }

            result = op == "*" ? result * rhs : result / rhs;
        }

        return result;
    }

    private static double ParseFactor(List<string> tokens, ref int position)
    {
        if (position >= tokens.Count)
        {
            throw new FormatException("Unexpected end of expression.");
        }

        var token = tokens[position];

        if (token == "-")
        {
            position++;
            return -ParseFactor(tokens, ref position);
        }

        if (token == "(")
        {
            position++;
            var result = ParseExpression(tokens, ref position);

            if (position >= tokens.Count || tokens[position] != ")")
            {
                throw new FormatException("Missing closing parenthesis.");
            }
            position++;
            return result;
        }

        if (double.TryParse(token, out var value))
        {
            position++;
            return value;
        }

        throw new FormatException($"Unexpected token '{token}' in expression.");
    }
}
