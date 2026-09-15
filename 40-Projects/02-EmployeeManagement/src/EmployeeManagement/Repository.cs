namespace EmployeeManagement;

/// <summary>
/// A minimal generic, in-memory repository. See module 08-Advanced-CSharp for why a
/// generic constraint (here, `where T : Employee`) gives compile-time safety without
/// boxing, unlike a pre-generics ArrayList-based design.
/// </summary>
public class Repository<T> where T : Employee
{
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    public T? FindById(Guid id) => _items.FirstOrDefault(e => e.Id == id);

    public IEnumerable<T> Find(Func<T, bool> predicate) => _items.Where(predicate);

    public int Count => _items.Count;
}
