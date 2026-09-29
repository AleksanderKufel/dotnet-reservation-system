using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ReservationSystem.Application.Exceptions;
using System.Net;

namespace ReservationSystem.API.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");

        var (statusCode, title) = exception switch
        {
            ArgumentException =>
                ((int)HttpStatusCode.BadRequest, "Invalid request"),

            NotFoundException =>
                ((int)HttpStatusCode.NotFound, "Not found"),

            ReservationConflictException =>
                ((int)HttpStatusCode.Conflict, "Reservation conflict"),

            _ when IsSerializationFailure(exception) =>
                ((int)HttpStatusCode.Conflict, "Concurrent booking conflict"),

            InvalidOperationException =>
                ((int)HttpStatusCode.BadRequest, "Business rule violation"),

            _ =>
                ((int)HttpStatusCode.InternalServerError, "Internal server error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= 500 && !environment.IsDevelopment()
                ? "An unexpected error occurred."
                : exception.Message
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    // EF Core may wrap the PostgreSQL serialization failure (40001) in another exception.
    private static bool IsSerializationFailure(Exception? exception)
    {
        for (; exception is not null; exception = exception.InnerException)
        {
            if (exception is NpgsqlException { SqlState: "40001" })
                return true;
        }

        return false;
    }
}