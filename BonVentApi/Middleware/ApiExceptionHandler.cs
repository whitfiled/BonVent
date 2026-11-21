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
        var (statusCode, code, message) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "not_found", "The requested resource was not found."),
            ConflictException => (StatusCodes.Status409Conflict, "conflict", "The request conflicts with the current state."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "unauthorized", "You are not authorized."),
            ArgumentException => (StatusCodes.Status400BadRequest, "invalid_request", "The request is invalid."),
            InvalidOperationException => (StatusCodes.Status409Conflict, "invalid_operation", "The operation cannot be completed."),
            _ => (0, string.Empty, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(code, message), cancellationToken);
        return true;
    }
}
