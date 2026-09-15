using EmployeeManagement;
using Xunit;

namespace EmployeeManagement.Tests;

public class ReportsTests
{
    private static readonly List<Employee> SampleEmployees = new()
    {
        new Employee("A", "Engineering", 100_000m),
        new Employee("B", "Engineering", 80_000m),
        new Employee("C", "Sales", 60_000m),
    };

    [Fact]
    public void HeadcountByDepartment_GroupsCorrectly()
    {
        var result = Reports.HeadcountByDepartment(SampleEmployees);

        Assert.Equal(2, result["Engineering"]);
        Assert.Equal(1, result["Sales"]);
    }

    [Fact]
    public void AverageSalary_ComputesCorrectAverage()
    {
        var average = Reports.AverageSalary(SampleEmployees);
        Assert.Equal(80_000m, average);
    }

    [Fact]
    public void AverageSalary_WithNoEmployees_ReturnsZero()
    {
        Assert.Equal(0m, Reports.AverageSalary(Enumerable.Empty<Employee>()));
    }

    [Fact]
    public void TopEarners_ReturnsHighestPaidFirst()
    {
        var top = Reports.TopEarners(SampleEmployees, 2);

        Assert.Equal(2, top.Count);
        Assert.Equal("A", top[0].Name);
        Assert.Equal("B", top[1].Name);
    }
}
