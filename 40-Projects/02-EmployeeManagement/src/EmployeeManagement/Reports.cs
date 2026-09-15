namespace EmployeeManagement;

/// <summary>LINQ-based reporting over a collection of employees. See module 10-LINQ.</summary>
public static class Reports
{
    public static IReadOnlyDictionary<string, int> HeadcountByDepartment(IEnumerable<Employee> employees) =>
        employees
            .GroupBy(e => e.Department)
            .ToDictionary(g => g.Key, g => g.Count());

    public static decimal AverageSalary(IEnumerable<Employee> employees)
    {
        var list = employees.ToList();
        return list.Count == 0 ? 0m : list.Average(e => e.Salary);
    }

    public static IReadOnlyList<Employee> TopEarners(IEnumerable<Employee> employees, int count) =>
        employees
            .OrderByDescending(e => e.Salary)
            .Take(count)
            .ToList();
}
