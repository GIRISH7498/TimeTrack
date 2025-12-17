using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Domain.Exceptions;

namespace TimeTrack.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred while processing request. TraceId: {TraceId}, Path: {Path}",
                traceId,
                context.Request.Path);

            //if (context.Response.HasStarted)
            //{
            //    _logger.LogWarning(
            //        "The response has already started, the exception handling middleware will not be executed.");
            //    throw;
            //}

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        string title = "An unexpected error occurred.";
        string? detail = null;
        string type = "https://httpstatuses.io/500";
        object? errors = null;

        switch (exception)
        {
            case ValidationException appValidationException:
                statusCode = HttpStatusCode.BadRequest;
                title = "One or more validation errors occurred.";
                type = "https://httpstatuses.io/400";
                errors = appValidationException.Errors;
                // for validation, we can show a generic message; detailed per-field errors go into `errors`
                detail = "Validation failed. See errors for details.";
                break;

            case FluentValidation.ValidationException fluentValidationException:
                statusCode = HttpStatusCode.BadRequest;
                title = "One or more validation errors occurred.";
                type = "https://httpstatuses.io/400";
                errors = fluentValidationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                detail = "Validation failed. See errors for details.";
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "The requested resource was not found.";
                type = "https://httpstatuses.io/404";
                detail = notFoundException.Message;   // safe, no stack trace
                break;

            case ForbiddenAccessException:
                statusCode = HttpStatusCode.Forbidden;
                title = "You do not have permission to perform this action.";
                type = "https://httpstatuses.io/403";
                detail = "You do not have permission to perform this action.";
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                title = "You are not authorized to perform this action.";
                type = "https://httpstatuses.io/401";
                detail = "You are not authorized to perform this action.";
                break;

            case DomainException domainException:
                statusCode = HttpStatusCode.BadRequest;
                title = "A business rule was violated.";
                type = "https://httpstatuses.io/400";
                detail = domainException.Message;     // safe, no stack trace
                break;
        }

        // For unknown exceptions, keep detail generic in ALL environments
        if (detail is null)
        {
            detail = "An unexpected error occurred. Please contact support with the provided traceId.";
        }

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        // Always add traceId so you can match logs
        problemDetails.Extensions["traceId"] = traceId;

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
