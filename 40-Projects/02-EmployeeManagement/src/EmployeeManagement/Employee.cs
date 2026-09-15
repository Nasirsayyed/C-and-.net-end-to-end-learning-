namespace EmployeeManagement;

/// <summary>
/// Base of the Employee → Manager → Executive hierarchy. See module 06-OOP for the
/// concepts this class demonstrates: encapsulation (private setters), inheritance,
/// and virtual/override-based runtime polymorphism via CalculateBonus().
/// </summary>
public class Employee
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public string Department { get; }
    public decimal Salary { get; private set; }

    public Employee(string name, string department, decimal salary)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }
        if (salary < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(salary), "Salary cannot be negative.");
        }

        Name = name;
        Department = department;
        Salary = salary;
    }

    public void GiveRaise(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Raise amount cannot be negative.");
        }
        Salary += amount;
    }

    /// <summary>Base bonus policy: 5% of salary. Overridden further down the hierarchy.</summary>
    public virtual decimal CalculateBonus() => Salary * 0.05m;

    public override string ToString() => $"{Name} ({Department}) — {GetType().Name}";
}
