namespace EmployeeManagement;

public class Executive : Manager
{
    public decimal StockGrantValue { get; }

    public Executive(string name, string department, decimal salary, int teamSize, decimal stockGrantValue)
        : base(name, department, salary, teamSize)
    {
        if (stockGrantValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockGrantValue), "Stock grant value cannot be negative.");
        }
        StockGrantValue = stockGrantValue;
    }

    /// <summary>Executives earn 20% of salary plus the full value of their stock grant.</summary>
    public override decimal CalculateBonus() => Salary * 0.20m + StockGrantValue;
}
