using EmployeeManagement;
using Xunit;

namespace EmployeeManagement.Tests;

public class RepositoryTests
{
    [Fact]
    public void Add_ThenGetAll_ReturnsAddedItem()
    {
        var repository = new Repository<Employee>();
        var employee = new Employee("Test", "Engineering", 50_000m);

        repository.Add(employee);

        Assert.Single(repository.GetAll());
        Assert.Same(employee, repository.GetAll()[0]);
    }

    [Fact]
    public void FindById_WithUnknownId_ReturnsNull()
    {
        var repository = new Repository<Employee>();
        Assert.Null(repository.FindById(Guid.NewGuid()));
    }

    [Fact]
    public void FindById_WithKnownId_ReturnsMatchingEmployee()
    {
        var repository = new Repository<Employee>();
        var employee = new Employee("Test", "Engineering", 50_000m);
        repository.Add(employee);

        var found = repository.FindById(employee.Id);

        Assert.Same(employee, found);
    }

    [Fact]
    public void Find_WithPredicate_ReturnsMatchingItemsOnly()
    {
        var repository = new Repository<Employee>();
        repository.Add(new Employee("A", "Engineering", 50_000m));
        repository.Add(new Employee("B", "Sales", 60_000m));

        var engineers = repository.Find(e => e.Department == "Engineering");

        Assert.Single(engineers);
    }

    [Fact]
    public void Repository_AcceptsSubtypes_ViaGenericConstraint()
    {
        // Compiles specifically because Repository<T> constrains T to Employee (or a subtype).
        var repository = new Repository<Manager>();
        repository.Add(new Manager("Test", "Engineering", 100_000m, teamSize: 3));
        Assert.Equal(1, repository.Count);
    }
}
