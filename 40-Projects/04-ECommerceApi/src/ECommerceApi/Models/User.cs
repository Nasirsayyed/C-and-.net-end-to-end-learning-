namespace ECommerceApi.Models;

public enum UserRole
{
    Customer,
    Admin
}

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; init; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; init; } = UserRole.Customer;
}
