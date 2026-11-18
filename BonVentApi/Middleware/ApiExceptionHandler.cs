using Application.Common.Exceptions;
using Contracts.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace BonVentApi.Middleware;

internal sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, code) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "not_found"),
            ConflictException => (StatusCodes.Status409Conflict, "conflict"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "unauthorized"),
            ArgumentException => (StatusCodes.Status400BadRequest, "invalid_request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "invalid_operation"),
            _ => (0, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(code, exception.Message), cancellationToken);
        return true;
    }
}
