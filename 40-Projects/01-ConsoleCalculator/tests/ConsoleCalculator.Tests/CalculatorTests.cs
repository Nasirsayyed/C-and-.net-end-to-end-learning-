using ConsoleCalculator;
using Xunit;

namespace ConsoleCalculator.Tests;

public class CalculatorTests
{
    private readonly Calculator _calculator = new();

    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("6 * 7", 42)]
    [InlineData("20 / 4", 5)]
    public void Evaluate_SingleOperator_ReturnsCorrectResult(string expression, double expected)
    {
        Assert.Equal(expected, _calculator.Evaluate(expression));
    }

    [Fact]
    public void Evaluate_RespectsOperatorPrecedence()
    {
        Assert.Equal(14, _calculator.Evaluate("2 + 3 * 4"));
    }

    [Fact]
    public void Evaluate_RespectsParentheses()
    {
        Assert.Equal(20, _calculator.Evaluate("(2 + 3) * 4"));
    }

    [Fact]
    public void Evaluate_SupportsUnaryMinus()
    {
        Assert.Equal(-5, _calculator.Evaluate("-5"));
        Assert.Equal(-1, _calculator.Evaluate("2 + -3"));
    }

    [Fact]
    public void Evaluate_SupportsNestedParentheses()
    {
        Assert.Equal(2, _calculator.Evaluate("((1 + 1) * (3 - 2))"));
    }

    [Fact]
    public void Evaluate_SupportsDecimals()
    {
        Assert.Equal(3.5, _calculator.Evaluate("1.5 + 2"));
    }

    [Fact]
    public void Evaluate_DivisionByZero_ThrowsDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => _calculator.Evaluate("5 / 0"));
    }

    [Fact]
    public void Evaluate_MissingClosingParenthesis_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _calculator.Evaluate("(1 + 2"));
    }

    [Fact]
    public void Evaluate_InvalidCharacter_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => _calculator.Evaluate("2 + a"));
    }

    [Fact]
    public void Evaluate_EmptyExpression_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.Evaluate(""));
    }
}
