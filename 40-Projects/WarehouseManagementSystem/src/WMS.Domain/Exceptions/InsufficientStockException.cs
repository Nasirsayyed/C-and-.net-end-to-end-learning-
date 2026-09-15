namespace WMS.Domain.Exceptions;

/// <summary>
/// A domain exception — see module 11-Exception-Handling. Carries structured data
/// (Sku, Requested, Available) rather than only a message, so callers (the Application
/// layer, or a global exception handler in the API) can react to it programmatically.
/// </summary>
public class InsufficientStockException : Exception
{
    public string Sku { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException(string sku, int requested, int available)
        : base($"Insufficient stock for '{sku}': requested {requested}, only {available} available.")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }
}
