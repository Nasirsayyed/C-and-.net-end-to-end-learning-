namespace EmployeeManagement;

public class Manager : Employee
{
    public int TeamSize { get; private set; }

    public Manager(string name, string department, decimal salary, int teamSize)
        : base(name, department, salary)
    {
        if (teamSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(teamSize), "Team size cannot be negative.");
        }
        TeamSize = teamSize;
    }

    public void AddDirectReport() => TeamSize++;

    /// <summary>Managers earn 10% of salary plus $100 per direct report.</summary>
    public override decimal CalculateBonus() => Salary * 0.10m + TeamSize * 100m;
}
