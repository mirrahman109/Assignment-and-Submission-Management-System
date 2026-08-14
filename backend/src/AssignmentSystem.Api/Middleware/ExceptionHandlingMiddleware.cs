using System.Net;
using AssignmentSystem.Api.Common;
using FluentValidation;

namespace AssignmentSystem.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, errors) = ex switch
        {
            NotFoundException e => (HttpStatusCode.NotFound, e.Message, (IDictionary<string, string[]>?)null),
            UnauthorizedAppException e => (HttpStatusCode.Unauthorized, e.Message, null),
            ForbiddenException e => (HttpStatusCode.Forbidden, e.Message, null),
            ConflictException e => (HttpStatusCode.Conflict, e.Message, null),
            ValidationAppException e => (HttpStatusCode.BadRequest, e.Message, e.Errors),
            ValidationException e => (HttpStatusCode.BadRequest, "Validation failed",
                e.Errors.GroupBy(x => x.PropertyName).ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{StatusCode} handling {Method} {Path}: {Message}",
                (int)statusCode, context.Request.Method, context.Request.Path, ex.Message);
        }

        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            title,
            status = (int)statusCode,
            errors
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
