using WMS.Domain.Exceptions;

namespace WMS.Api;

/// <summary>
/// Centralizes exception-to-HTTP-response mapping in one place (see modules
/// 11-Exception-Handling and 17-Middleware) instead of a try/catch per endpoint.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning(ex, "Insufficient stock for {Sku}", ex.Sku);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            status = statusCode,
            detail
        });
    }
}
