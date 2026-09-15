using System.Globalization;
using EmployeeManagement;

CultureInfo.CurrentCulture = new CultureInfo("en-US"); // deterministic currency formatting regardless of host locale

var repository = new Repository<Employee>();

repository.Add(new Employee("Ada Lovelace", "Engineering", 95_000m));
repository.Add(new Employee("Grace Hopper", "Engineering", 98_000m));
repository.Add(new Manager("Alan Turing", "Engineering", 130_000m, teamSize: 6));
repository.Add(new Executive("Margaret Hamilton", "Engineering", 220_000m, teamSize: 20, stockGrantValue: 50_000m));
repository.Add(new Employee("Katherine Johnson", "Analytics", 90_000m));

Console.WriteLine("=== Bonuses (polymorphic — same call, different behavior per subtype) ===");
foreach (var employee in repository.GetAll())
{
    Console.WriteLine($"{employee,-40} bonus = {employee.CalculateBonus():C}");
}

Console.WriteLine();
Console.WriteLine("=== Headcount by department ===");
foreach (var (department, count) in Reports.HeadcountByDepartment(repository.GetAll()))
{
    Console.WriteLine($"{department}: {count}");
}

Console.WriteLine();
Console.WriteLine($"Average salary: {Reports.AverageSalary(repository.GetAll()):C}");

Console.WriteLine();
Console.WriteLine("=== Top 3 earners ===");
foreach (var employee in Reports.TopEarners(repository.GetAll(), 3))
{
    Console.WriteLine($"{employee} — {employee.Salary:C}");
}
