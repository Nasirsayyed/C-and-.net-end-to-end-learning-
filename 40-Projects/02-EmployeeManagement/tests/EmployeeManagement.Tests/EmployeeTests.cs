using EmployeeManagement;
using Xunit;

namespace EmployeeManagement.Tests;

public class EmployeeTests
{
    [Fact]
    public void CalculateBonus_ForBaseEmployee_Is5PercentOfSalary()
    {
        var employee = new Employee("Test", "Engineering", 100_000m);
        Assert.Equal(5_000m, employee.CalculateBonus());
    }

    [Fact]
    public void CalculateBonus_ForManager_Includes10PercentPlusPerReportBonus()
    {
        var manager = new Manager("Test", "Engineering", 100_000m, teamSize: 5);
        Assert.Equal(10_000m + 500m, manager.CalculateBonus());
    }

    [Fact]
    public void CalculateBonus_ForExecutive_Includes20PercentPlusStockGrant()
    {
        var executive = new Executive("Test", "Engineering", 200_000m, teamSize: 10, stockGrantValue: 30_000m);
        Assert.Equal(40_000m + 30_000m, executive.CalculateBonus());
    }

    [Fact]
    public void CalculateBonus_IsPolymorphic_WhenCalledThroughBaseReference()
    {
        Employee manager = new Manager("Test", "Engineering", 100_000m, teamSize: 5);
        // Called through an Employee-typed reference, but the runtime type's override still runs.
        Assert.Equal(10_500m, manager.CalculateBonus());
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Employee("", "Engineering", 50_000m));
    }

    [Fact]
    public void Constructor_WithNegativeSalary_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Employee("Test", "Engineering", -1m));
    }

    [Fact]
    public void GiveRaise_IncreasesSalary()
    {
        var employee = new Employee("Test", "Engineering", 50_000m);
        employee.GiveRaise(5_000m);
        Assert.Equal(55_000m, employee.Salary);
    }

    [Fact]
    public void GiveRaise_WithNegativeAmount_Throws()
    {
        var employee = new Employee("Test", "Engineering", 50_000m);
        Assert.Throws<ArgumentOutOfRangeException>(() => employee.GiveRaise(-100m));
    }
}
